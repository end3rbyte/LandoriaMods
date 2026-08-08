#!/usr/bin/env bash
set -euo pipefail

usage() {
    echo "Usage: $0 MOD [MOD ...]" >&2
    exit 2
}

[[ $# -gt 0 ]] || usage

repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
# shellcheck source-path=SCRIPTDIR
# shellcheck source=changelog.sh
source "$repository_root/scripts/ci/changelog.sh"
# shellcheck source-path=SCRIPTDIR
# shellcheck source=release-version.sh
source "$repository_root/scripts/ci/release-version.sh"
thunderstore_url="${THUNDERSTORE_URL:-https://thunderstore.io}"
internal_url="${LANDORIA_MOD_REPOSITORY_URL:-https://test.landoria-gaming.com:8443/api/v1/packages}"
thunderstore_environment="${THUNDERSTORE_SECRET_ENVIRONMENT:-/var/lib/landoria-secrets/thunderstore-publish.env}"
internal_environment="${LANDORIA_MOD_REPOSITORY_SECRET_ENVIRONMENT:-/var/lib/landoria-secrets/mod-repository-upload.env}"
tcli="${TCLI_COMMAND:-$repository_root/artifacts/tools/tcli}"
modpack_configuration="${LANDORIA_MODPACK_CONFIGURATION_JSON:?LANDORIA_MODPACK_CONFIGURATION_JSON is required}"

require_command() {
    command -v "$1" >/dev/null 2>&1 || {
        echo "Required command not found: $1" >&2
        exit 1
    }
}

package_category() {
    local package="$1"
    [[ "$package" == LandoriaModPack ]] && { echo modpack; return; }
    jq -er --arg package "$package" '
        if (.landoria_packages | index($package)) != null then "modpack"
        elif (.standalone_packages | index($package)) != null then "standalone"
        elif (.server_only_packages | index($package)) != null then "server-only"
        else error("package is absent from the central inventory") end
    ' <<< "$modpack_configuration"
}

read_secret() {
    local file="$1" key="$2" value
    [[ -r "$file" ]] || { echo "Vault Agent output is unavailable: $file" >&2; return 1; }
    value="$(sed -n "s/^${key}=//p" "$file")"
    [[ -n "$value" && "$value" != *$'\n'* ]] || { echo "$key is missing or duplicated in $file." >&2; return 1; }
    printf '%s' "$value"
}

thunderstore_latest() {
    local package="$1" response status body
    response="$(curl --retry 5 --retry-all-errors --retry-delay 2 --silent --show-error \
        --write-out $'\n%{http_code}' "$thunderstore_url/api/experimental/package/Landoria/$package/")"
    status="${response##*$'\n'}"
    body="${response%$'\n'*}"
    if [[ "$status" == 404 ]]; then return 0; fi
    [[ "$status" =~ ^2 ]] || { echo "Thunderstore lookup failed for $package ($status)." >&2; return 1; }
    jq -r '.latest.version_number // empty' <<< "$body"
}

has_package_changes() {
    local mod="$1" latest="$2" tag
    [[ -z "$latest" ]] && return 0
    tag="thunderstore/$mod/$latest"
    git -C "$repository_root" rev-parse --verify --quiet "refs/tags/$tag" >/dev/null || return 0
    ! git -C "$repository_root" diff --quiet "$tag" -- \
        Directory.Build.props scripts/ci/changelog.sh scripts/ci/publish-mods.sh \
        scripts/ci/prepare-build-dependencies.sh Landoria.SharedLib "Landoria.$mod" \
        ":(exclude)Landoria.SharedLib/README.md" ":(exclude)Landoria.SharedLib/LICENSE" \
        ":(exclude)Landoria.$mod/README.md" ":(exclude)Landoria.$mod/LICENSE"
}

write_tcli_config() {
    local directory="$1" target="$2" package version description website categories
    package="$(jq -r '.name' "$directory/manifest.json")"
    version="$(jq -r '.version_number' "$directory/manifest.json")"
    description="$(jq -r '.description | @json' "$directory/manifest.json")"
    website="$(jq -r '.website_url | @json' "$directory/manifest.json")"
    categories="$(jq -r '.categories | map(@json) | join(", ")' "$directory/manifest.json")"
    {
        printf '[config]\nschemaVersion = "0.0.1"\n\n'
        printf '[package]\nnamespace = "Landoria"\nname = "%s"\nversionNumber = "%s"\n' "$package" "$version"
        printf 'description = %s\nwebsiteUrl = %s\ncontainsNsfwContent = false\ndependencies = {}\n\n' "$description" "$website"
        printf '[publish]\nrepository = "%s"\ncommunities = ["valheim"]\n\n' "$thunderstore_url"
        printf '[publish.categories]\nvalheim = [%s]\n' "$categories"
    } > "$target"
}

thunderstore_release_exists() {
    local package="$1" version="$2"
    curl --fail --location --silent --output /dev/null \
        "$thunderstore_url/package/download/Landoria/$package/$version/"
}

confirm_thunderstore_release() {
    local package="$1" version="$2"
    for _ in {1..12}; do
        thunderstore_release_exists "$package" "$version" && return 0
        sleep 5
    done
    echo "Thunderstore did not expose the Landoria-$package-$version archive after publication." >&2
    return 1
}

mark_internal_release() {
    local package="$1" version="$2" api_key response
    api_key="$(read_secret "$internal_environment" Authentication__ApiKey)"
    response="$(printf 'header = "X-Api-Key: %s"\n' "$api_key" | curl --fail --silent --show-error \
        --retry 5 --retry-all-errors --retry-delay 2 --config - --request POST \
        "$internal_url/Landoria/$package/$version/release")"
    [[ "$(jq -r '.released' <<< "$response")" == true ]] || {
        echo "The internal package repository did not mark Landoria-$package-$version as released." >&2
        return 1
    }
}

internal_release_status() {
    local package="$1" version="$2" state
    state="$(curl --fail --silent --show-error --retry 5 --retry-all-errors --retry-delay 2 \
        "$internal_url/Landoria/$package")"
    jq -er --arg version "$version" \
        '.[] | select(.versionNumber == $version) | (.released | tostring)' <<< "$state"
}

download_internal_draft() {
    local package="$1" version="$2" target="$3" state metadata released expected_hash
    local actual_hash manifest_name manifest_version archived_changelog
    state="$(curl --fail --silent --show-error --retry 5 --retry-all-errors --retry-delay 2 \
        "$internal_url/Landoria/$package")"
    metadata="$(jq -ec --arg version "$version" \
        '.[] | select(.versionNumber == $version)' <<< "$state")" || {
        echo "The private draft Landoria-$package-$version does not exist." >&2
        return 1
    }
    released="$(jq -r '.released' <<< "$metadata")"
    [[ "$released" == false ]] || {
        echo "Landoria-$package-$version is already marked released in the private repository." >&2
        return 1
    }
    expected_hash="$(jq -r '.sha256' <<< "$metadata")"
    curl --fail --silent --show-error --location --retry 5 --retry-all-errors --retry-delay 2 \
        "$internal_url/Landoria/$package/$version/download" --output "$target"
    actual_hash="$(sha256sum "$target" | awk '{ print toupper($1) }')"
    [[ "$actual_hash" == "${expected_hash^^}" ]] || {
        echo "The downloaded draft hash does not match the private repository metadata for Landoria-$package-$version." >&2
        return 1
    }
    manifest_name="$(unzip -p "$target" manifest.json | jq -er '.name')"
    manifest_version="$(unzip -p "$target" manifest.json | jq -er '.version_number')"
    [[ "$manifest_name" == "$package" && "$manifest_version" == "$version" ]] || {
        echo "The private draft manifest does not match Landoria-$package-$version." >&2
        return 1
    }
    archived_changelog="$(mktemp)"
    unzip -p "$target" CHANGELOG.md > "$archived_changelog"
    validate_release_changelog "$archived_changelog" "$version"
    rm -f -- "$archived_changelog"
    echo "Validated immutable private draft Landoria-$package-$version ($actual_hash)."
}

for command in awk curl dotnet find git jq sed sha256sum unzip; do require_command "$command"; done
jq -e '
    type == "object" and
    (.landoria_packages | type == "array" and length > 0 and length == (unique | length)) and
    (.server_only_packages | type == "array" and length > 0 and length == (unique | length)) and
    (.standalone_packages | type == "array" and length > 0 and length == (unique | length)) and
    all([.landoria_packages[], .server_only_packages[], .standalone_packages[]][];
        type == "string" and test("^[A-Za-z0-9_]+$")) and
    ([.landoria_packages[], .server_only_packages[], .standalone_packages[]] | length) ==
    ([.landoria_packages[], .server_only_packages[], .standalone_packages[]] | unique | length)
' <<< "$modpack_configuration" >/dev/null || {
    echo "LANDORIA_MODPACK_CONFIGURATION_JSON is invalid." >&2
    exit 2
}
[[ -x "$tcli" ]] || { echo "TCLI is unavailable: $tcli" >&2; exit 1; }
[[ -z "$(git -C "$repository_root" status --porcelain)" ]] || {
    echo "Repository must be clean before promotion." >&2
    exit 1
}

mkdir -p "$repository_root/artifacts/thunderstore"
declare -a mods=() versions=() archives=() categories=()
declare -A seen=()
declare -a requested_mods=()
modpack_requested=false
for mod in "$@"; do
    if [[ "$mod" == LandoriaModPack ]]; then
        modpack_requested=true
    else
        requested_mods+=("$mod")
    fi
done
[[ "$modpack_requested" == false ]] || requested_mods+=(LandoriaModPack)

for mod in "${requested_mods[@]}"; do
    [[ "$mod" =~ ^[A-Za-z0-9]+$ ]] || usage
    [[ -d "$repository_root/Landoria.$mod" ]] || { echo "Unknown public mod: $mod" >&2; exit 1; }
    [[ -z "${seen[$mod]:-}" ]] || continue
    seen[$mod]=1
    package="$(jq -r '.name' "$repository_root/Landoria.$mod/manifest.json")"
    category="$(package_category "$package")" || {
        echo "Landoria-$package cannot be promoted because it is absent from the central package inventory." >&2
        exit 1
    }
    current="$(jq -r '.version_number' "$repository_root/Landoria.$mod/manifest.json")"
    archive="$repository_root/artifacts/thunderstore/$package-$current-private-draft.zip"

    if [[ "$category" == server-only ]]; then
        validate_release_changelog "$repository_root/Landoria.$mod/CHANGELOG.md" "$current"
        download_internal_draft "$package" "$current" "$archive"
        mods+=("$mod")
        versions+=("$current")
        archives+=("$archive")
        categories+=("$category")
        continue
    fi

    latest="$(thunderstore_latest "$package")"

    if thunderstore_release_exists "$package" "$current"; then
        internal_status="$(internal_release_status "$package" "$current")" || {
            echo "Landoria-$package-$current is missing from the private repository." >&2
            exit 1
        }
        [[ "$internal_status" == true ]] || mark_internal_release "$package" "$current"
        if ! git -C "$repository_root" rev-parse --verify --quiet \
            "refs/tags/thunderstore/$mod/$current" >/dev/null; then
            git -C "$repository_root" tag "thunderstore/$mod/$current"
            git -C "$repository_root" push origin "refs/tags/thunderstore/$mod/$current"
        fi
        echo "Landoria-$package-$current is already released; no package rebuild is required."
        continue
    fi

    if ! has_package_changes "$mod" "$latest"; then
        echo "Skipping Landoria-$package: its generated package inputs have not changed since $latest."
        continue
    fi
    validate_next_release_version "$package" "$current" "$latest"
    validate_release_changelog "$repository_root/Landoria.$mod/CHANGELOG.md" "$current"
    download_internal_draft "$package" "$current" "$archive"
    mods+=("$mod")
    versions+=("$current")
    archives+=("$archive")
    categories+=("$category")
done

if [[ ${#mods[@]} -eq 0 ]]; then
    echo "No changed packages require promotion."
    exit 0
fi

export TCLI_AUTH_TOKEN
TCLI_AUTH_TOKEN="$(read_secret "$thunderstore_environment" TCLI_AUTH_TOKEN)"
for index in "${!mods[@]}"; do
    mod="${mods[$index]}"
    directory="$repository_root/Landoria.$mod"
    package="$(jq -r '.name' "$directory/manifest.json")"
    version="${versions[$index]}"
    archive="${archives[$index]}"
    category="${categories[$index]}"
    if [[ "$category" == server-only ]]; then
        mark_internal_release "$package" "$version"
        tag="production/server-only/$mod/$version"
        git -C "$repository_root" tag "$tag"
        git -C "$repository_root" push origin "refs/tags/$tag"
        echo "Released private server-only package Landoria-$package-$version without publishing it on Thunderstore."
        continue
    fi
    config="$repository_root/artifacts/thunderstore/$package-$version.toml"
    write_tcli_config "$directory" "$config"
    "$tcli" publish --config-path "$config" --file "$archive"
    confirm_thunderstore_release "$package" "$version"
    mark_internal_release "$package" "$version"
    tag="thunderstore/$mod/$version"
    git -C "$repository_root" tag "$tag"
    git -C "$repository_root" push origin "refs/tags/$tag"
    echo "Released Landoria-$package-$version on Thunderstore and in Landoria's package repository."
done
