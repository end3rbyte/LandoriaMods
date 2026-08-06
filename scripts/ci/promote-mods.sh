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
thunderstore_url="${THUNDERSTORE_URL:-https://thunderstore.io}"
internal_url="${LANDORIA_MOD_REPOSITORY_URL:-https://test.landoria-gaming.com:8443/api/v1/packages}"
thunderstore_environment="${THUNDERSTORE_SECRET_ENVIRONMENT:-/var/lib/landoria-secrets/thunderstore-publish.env}"
internal_environment="${LANDORIA_MOD_REPOSITORY_SECRET_ENVIRONMENT:-/var/lib/landoria-secrets/mod-repository-upload.env}"
tcli="${TCLI_COMMAND:-$repository_root/artifacts/tools/tcli}"

require_command() {
    command -v "$1" >/dev/null 2>&1 || {
        echo "Required command not found: $1" >&2
        exit 1
    }
}

read_secret() {
    local file="$1" key="$2" value
    [[ -r "$file" ]] || { echo "Vault Agent output is unavailable: $file" >&2; return 1; }
    value="$(sed -n "s/^${key}=//p" "$file")"
    [[ -n "$value" && "$value" != *$'\n'* ]] || { echo "$key is missing or duplicated in $file." >&2; return 1; }
    printf '%s' "$value"
}

plugin_file() {
    local directory="$1" files
    mapfile -t files < <(find "$directory" -maxdepth 1 -name '*Plugin.cs' -type f)
    [[ ${#files[@]} -eq 1 ]] || { echo "Expected one plugin entry point in $directory." >&2; return 1; }
    printf '%s\n' "${files[0]}"
}

replace_version() {
    local directory="$1" version="$2" plugin temporary
    plugin="$(plugin_file "$directory")"
    perl -0pi -e "s/(PluginVersion\\s*=\\s*\")\\d+\\.\\d+\\.\\d+(\";)/\${1}$version\${2}/" "$plugin"
    temporary="$directory/manifest.json.tmp"
    jq --arg version "$version" '.version_number = $version' "$directory/manifest.json" > "$temporary"
    mv -- "$temporary" "$directory/manifest.json"
    perl -0pi -e "s/(AssemblyVersion\\(\")\\d+\\.\\d+\\.\\d+(\\.\\*\"\\))/\${1}$version\${2}/; s/(AssemblyFileVersion\\(\")\\d+\\.\\d+\\.\\d+(\"\\))/\${1}$version\${2}/" \
        "$directory/Properties/AssemblyInfo.cs"
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

next_patch() {
    local version="$1" major minor patch
    if [[ -z "$version" ]]; then printf '1.0.0\n'; return; fi
    IFS=. read -r major minor patch <<< "$version"
    printf '%s.%s.%s\n' "$major" "$minor" "$((patch + 1))"
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

for command in curl dotnet find git jq perl sed; do require_command "$command"; done
[[ -x "$tcli" ]] || { echo "TCLI is unavailable: $tcli" >&2; exit 1; }
[[ -z "$(git -C "$repository_root" status --porcelain)" ]] || {
    echo "Repository must be clean before promotion." >&2
    exit 1
}

declare -a mods=() next_versions=()
declare -A seen=()
for mod in "$@"; do
    [[ "$mod" =~ ^[A-Za-z0-9]+$ ]] || usage
    [[ -d "$repository_root/Landoria.$mod" ]] || { echo "Unknown public mod: $mod" >&2; exit 1; }
    [[ -z "${seen[$mod]:-}" ]] || continue
    seen[$mod]=1
    package="$(jq -r '.name' "$repository_root/Landoria.$mod/manifest.json")"
    latest="$(thunderstore_latest "$package")"
    current="$(jq -r '.version_number' "$repository_root/Landoria.$mod/manifest.json")"

    if ! git -C "$repository_root" rev-parse --verify --quiet "refs/tags/thunderstore/$mod/$current" >/dev/null && \
       thunderstore_release_exists "$package" "$current"; then
        mark_internal_release "$package" "$current"
        git -C "$repository_root" tag "thunderstore/$mod/$current"
        git -C "$repository_root" push origin "refs/tags/thunderstore/$mod/$current"
        echo "Reconciled the existing Thunderstore release Landoria-$package-$current."
        continue
    fi

    if ! has_package_changes "$mod" "$latest"; then
        echo "Skipping Landoria-$package: its generated package inputs have not changed since $latest."
        continue
    fi
    version="$(next_patch "$latest")"
    validate_release_changelog "$repository_root/Landoria.$mod/CHANGELOG.md" "$version"
    replace_version "$repository_root/Landoria.$mod" "$version"
    git -C "$repository_root" add -- "Landoria.$mod"
    mods+=("$mod")
    next_versions+=("$version")
done

if [[ ${#mods[@]} -eq 0 ]]; then
    echo "No changed packages require promotion."
    exit 0
fi

if ! git -C "$repository_root" diff --cached --quiet; then
    git -C "$repository_root" commit -m "Prepare Thunderstore releases" \
        -m 'Thunderstore-Release: true' -m 'Release-Version-Bump: true'
    git -C "$repository_root" push origin HEAD:main
fi
"$repository_root/scripts/ci/publish-mods.sh" --no-version-bump "${mods[@]}"

export TCLI_AUTH_TOKEN
TCLI_AUTH_TOKEN="$(read_secret "$thunderstore_environment" TCLI_AUTH_TOKEN)"
for index in "${!mods[@]}"; do
    mod="${mods[$index]}"
    directory="$repository_root/Landoria.$mod"
    package="$(jq -r '.name' "$directory/manifest.json")"
    version="${next_versions[$index]}"
    archive="$(find "$repository_root/artifacts/thunderstore" -maxdepth 1 -type f \
        -name "$package-$version-*.zip" -print -quit)"
    [[ -n "$archive" ]] || { echo "Generated archive not found for $package $version." >&2; exit 1; }
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
