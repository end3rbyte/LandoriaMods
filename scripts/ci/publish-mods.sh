#!/usr/bin/env bash
set -euo pipefail

usage() {
    echo "Usage: $0 [--no-version-bump] MOD [MOD ...]" >&2
    exit 2
}

bump_versions=true
if [[ "${1:-}" == "--no-version-bump" ]]; then
    bump_versions=false
    shift
fi
[[ $# -gt 0 ]] || usage

repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
# shellcheck source-path=SCRIPTDIR
# shellcheck source=changelog.sh
source "$repository_root/scripts/ci/changelog.sh"
dependency_script="$repository_root/scripts/ci/prepare-build-dependencies.sh"
output="$repository_root/artifacts/thunderstore"
modpack_directory="$repository_root/Landoria.LandoriaModPack"
modpack_manifest="$modpack_directory/manifest.json"
api_url="${LANDORIA_MOD_REPOSITORY_URL:-https://test.landoria-gaming.com:8443/api/v1/packages}"
secret_environment="${LANDORIA_MOD_REPOSITORY_SECRET_ENVIRONMENT:-/var/lib/landoria-secrets/mod-repository-upload.env}"

require_command() {
    command -v "$1" >/dev/null 2>&1 || {
        echo "Required command not found: $1" >&2
        exit 1
    }
}

plugin_file() {
    local directory="$1" files
    mapfile -t files < <(find "$directory" -maxdepth 1 -name '*Plugin.cs' -type f)
    [[ ${#files[@]} -eq 1 ]] || {
        echo "Expected exactly one plugin entry point in $directory." >&2
        return 1
    }
    printf '%s\n' "${files[0]}"
}

read_plugin_version() {
    grep -Eo 'PluginVersion[^\"]*\"[0-9]+\.[0-9]+\.[0-9]+\"' "$1" |
        head -n 1 | grep -Eo '[0-9]+\.[0-9]+\.[0-9]+'
}

repository_state() {
    local package="$1" response status body
    response="$(curl --retry 5 --retry-all-errors --retry-delay 2 \
        --silent --show-error --write-out $'\n%{http_code}' "$api_url/Landoria/$package")"
    status="${response##*$'\n'}"
    body="${response%$'\n'*}"
    if [[ "$status" == 404 ]]; then
        printf '\t\n'
        return
    fi
    [[ "$status" =~ ^2 ]] || {
        echo "Package repository lookup failed for $package ($status)." >&2
        return 1
    }
    jq -r 'sort_by(.versionNumber | split(".") | map(tonumber)) | last |
        [.versionNumber, (.released | tostring)] | @tsv' <<< "$body"
}

next_version() {
    local local_version="$1" repository_version="$2" latest major minor patch
    latest="$local_version"
    if [[ -n "$repository_version" ]] && [[ "$(printf '%s\n%s\n' "$local_version" "$repository_version" | sort -V | tail -n 1)" == "$repository_version" ]]; then
        latest="$repository_version"
    fi
    IFS=. read -r major minor patch <<< "$latest"
    printf '%s.%s.%s\n' "$major" "$minor" "$((patch + 1))"
}

replace_version() {
    local directory="$1" plugin="$2" version="$3" temporary
    perl -0pi -e "s/(PluginVersion\\s*=\\s*\")\\d+\\.\\d+\\.\\d+(\";)/\${1}$version\${2}/" "$plugin"
    temporary="$directory/manifest.json.tmp"
    jq --arg version "$version" '.version_number = $version' "$directory/manifest.json" > "$temporary"
    mv -- "$temporary" "$directory/manifest.json"
    perl -0pi -e "s/(AssemblyVersion\\(\")\\d+\\.\\d+\\.\\d+(\\.\\*\"\\))/\${1}$version\${2}/; s/(AssemblyFileVersion\\(\")\\d+\\.\\d+\\.\\d+(\"\\))/\${1}$version\${2}/" \
        "$directory/Properties/AssemblyInfo.cs"
}

modpack_tracks_package() {
    local package="$1"
    jq -e --arg prefix "Landoria-$package-" \
        '.dependencies[]? | select(startswith($prefix))' "$modpack_manifest" >/dev/null
}

sync_modpack_dependency() {
    local package="$1" version="$2" prefix dependency current temporary
    prefix="Landoria-$package-"
    dependency="Landoria-$package-$version"
    current="$(jq -r --arg prefix "$prefix" \
        '.dependencies[]? | select(startswith($prefix))' "$modpack_manifest")"
    [[ -n "$current" ]] || return 1
    [[ "$current" != *$'\n'* ]] || {
        echo "Duplicate LandoriaModPack dependency for $package." >&2
        exit 1
    }
    [[ "$current" != "$dependency" ]] || return 1
    temporary="$modpack_manifest.tmp"
    jq --arg prefix "$prefix" --arg dependency "$dependency" \
        '.dependencies |= map(if startswith($prefix) then $dependency else . end)' \
        "$modpack_manifest" > "$temporary"
    mv -- "$temporary" "$modpack_manifest"
    git -C "$repository_root" add -- Landoria.LandoriaModPack/manifest.json
    echo "Updated LandoriaModPack dependency to $dependency."
}

validate_metadata() {
    local mod="$1" directory="$2" plugin="$3" version plugin_version assembly_version file_version
    local expected_website valid_categories
    for file in "$directory/manifest.json" "$plugin" "$directory/Properties/AssemblyInfo.cs" \
        "$directory/icon.png" "$directory/LICENSE" "$directory/README.md" \
        "$directory/CHANGELOG.md" \
        "$directory/README.Thunderstore.md"; do
        [[ -f "$file" ]] || { echo "Required file not found: $file" >&2; return 1; }
    done
    expected_website="https://github.com/end3rbyte/LandoriaMods/tree/main/Landoria.$mod"
    valid_categories='["ai-generated","ashlands-update","audio","bog-witch-update","building","client-side","crafting","enemies","gear","hildirs-request-update","language","libraries","misc","mistlands-update","modpacks","mods","npcs","pvp","server-side","skins","tools","transportation","tweaks","utility","vehicles","world-generation"]'
    jq -e --arg website "$expected_website" --argjson valid "$valid_categories" '
        (.name | test("^[A-Za-z0-9_]+$")) and
        (.website_url == $website) and
        (.description | length > 0 and length < 150) and
        (.categories | type == "array" and length > 0 and
            index("ai-generated") != null and length == (unique | length)) and
        all(.categories[]; . as $category | $valid | index($category) != null) and
        (.dependencies | type == "array" and length > 0 and
            length == (unique | length)) and
        all(.dependencies[]; test("^[A-Za-z0-9_]+-[A-Za-z0-9_]+-[0-9]+\\.[0-9]+\\.[0-9]+$")) and
        (.dependencies | index("denikson-BepInExPack_Valheim-5.4.2333") != null)
    ' "$directory/manifest.json" >/dev/null || {
        echo "Manifest metadata is invalid for $mod." >&2
        return 1
    }
    grep -Fq "https://github.com/end3rbyte/LandoriaMods/blob/main/Landoria.$mod/README.md" \
        "$directory/README.Thunderstore.md" || {
        echo "The Thunderstore README has no valid full-documentation link for $mod." >&2
        return 1
    }
    file "$directory/icon.png" | grep -Fq 'PNG image data, 256 x 256' || {
        echo "The icon must be a 256 by 256 PNG for $mod." >&2
        return 1
    }
    version="$(jq -r '.version_number' "$directory/manifest.json")"
    plugin_version="$(read_plugin_version "$plugin")"
    assembly_version="$(grep -Eo 'AssemblyVersion\("[0-9]+\.[0-9]+\.[0-9]+\.(\*|[0-9]+)"' "$directory/Properties/AssemblyInfo.cs" | head -1 | grep -Eo '[0-9]+\.[0-9]+\.[0-9]+')"
    file_version="$(grep -Eo 'AssemblyFileVersion\("[0-9]+\.[0-9]+\.[0-9]+"' "$directory/Properties/AssemblyInfo.cs" | head -1 | grep -Eo '[0-9]+\.[0-9]+\.[0-9]+')"
    [[ "$version" == "$plugin_version" && "$version" == "$assembly_version" && "$version" == "$file_version" ]] || {
        echo "Version metadata does not match for $mod." >&2
        return 1
    }
}

create_archive() {
    local mod="$1" directory="$2" commit="$3" name version staging archive
    name="$(jq -r '.name' "$directory/manifest.json")"
    version="$(jq -r '.version_number' "$directory/manifest.json")"
    staging="$output/$name-$version-$commit"
    archive="$staging.zip"
    rm -rf -- "$staging" "$archive"
    mkdir -p -- "$staging"
    cp -- "$directory/bin/Release/Landoria.$mod.dll" "$directory/icon.png" "$staging/"
    jq --arg commit "$commit" '. + {commit_id: $commit}' "$directory/manifest.json" > "$staging/manifest.json"
    cp -- "$directory/README.Thunderstore.md" "$staging/README.md"
    cp -- "$directory/CHANGELOG.md" "$staging/CHANGELOG.md"
    (cd -- "$staging" && zip -q "$archive" ./*)
    rm -rf -- "$staging"
    printf '%s\n' "$archive"
}

upload_archive() {
    local directory="$1" archive="$2" categories api_key result released
    [[ -r "$secret_environment" ]] || {
        echo "The Vault Agent API environment is unavailable: $secret_environment" >&2
        return 1
    }
    api_key="$(sed -n 's/^Authentication__ApiKey=//p' "$secret_environment")"
    [[ -n "$api_key" ]] || { echo "Authentication__ApiKey is missing." >&2; return 1; }
    categories="$(jq -r '.categories | join(",")' "$directory/manifest.json")"
    result="$(printf 'header = "X-Api-Key: %s"\n' "$api_key" | curl --fail \
        --retry 5 --retry-all-errors --retry-delay 2 --silent --show-error --config - \
        --form 'namespace=Landoria' --form "categories=$categories" \
        --form "package=@$archive;type=application/zip" "$api_url")"
    released="$(jq -r '.released' <<< "$result")"
    [[ "$released" == false ]] || { echo "The uploaded version was unexpectedly released." >&2; return 1; }
}

for command in curl dotnet file find git jq perl sed sort unzip zip; do
    require_command "$command"
done
[[ -z "$(git -C "$repository_root" status --porcelain)" ]] || {
    echo "Repository must be clean before publishing." >&2
    exit 1
}

declare -a requested_mods mods directories plugins
declare -A seen_mods=()
include_modpack=false
for mod in "$@"; do
    [[ "$mod" =~ ^[A-Za-z0-9]+$ ]] || usage
    directory="$repository_root/Landoria.$mod"
    [[ -d "$directory" ]] || { echo "Unknown public mod: $mod" >&2; exit 1; }
    [[ -z "${seen_mods[$mod]:-}" ]] || continue
    seen_mods["$mod"]=1
    requested_mods+=("$mod")
    if [[ "$mod" == LandoriaModPack ]]; then
        include_modpack=true
        continue
    fi
    package="$(jq -r '.name' "$directory/manifest.json")"
    modpack_tracks_package "$package" && include_modpack=true
done
if [[ "$include_modpack" == true && -z "${seen_mods[LandoriaModPack]:-}" ]]; then
    requested_mods+=(LandoriaModPack)
fi

for mod in "${requested_mods[@]}"; do
    directory="$repository_root/Landoria.$mod"
    plugin="$(plugin_file "$directory")"
    validate_metadata "$mod" "$directory" "$plugin"
    mods+=("$mod")
    directories+=("$directory")
    plugins+=("$plugin")
done

if [[ "$bump_versions" == true ]]; then
    versions_changed=false
    for index in "${!mods[@]}"; do
        package="$(jq -r '.name' "${directories[$index]}/manifest.json")"
        current="$(read_plugin_version "${plugins[$index]}")"
        IFS=$'\t' read -r published released < <(repository_state "$package")
        version="$current"
        if [[ -z "$published" ]] || \
            { [[ "$current" != "$published" ]] && [[ "$(printf '%s\n%s\n' "$current" "$published" | sort -V | tail -n 1)" == "$current" ]]; } || \
            { [[ "$current" == "$published" ]] && [[ "$released" == false ]]; }; then
            echo "$package $current can be published without a new version."
        else
            version="$(next_version "$current" "$published")"
            replace_version "${directories[$index]}" "${plugins[$index]}" "$version"
            validate_metadata "${mods[$index]}" "${directories[$index]}" "${plugins[$index]}"
            git -C "$repository_root" add -- "Landoria.${mods[$index]}"
            versions_changed=true
        fi
        update_unreleased_changelog "$repository_root" "${mods[$index]}" "$version"
        if ! git -C "$repository_root" diff --quiet -- "Landoria.${mods[$index]}/CHANGELOG.md"; then
            git -C "$repository_root" add -- "Landoria.${mods[$index]}/CHANGELOG.md"
            versions_changed=true
        fi
        if [[ "${mods[$index]}" != LandoriaModPack ]] &&
            sync_modpack_dependency "$package" "$version"; then
            versions_changed=true
        fi
    done
    if [[ "$versions_changed" == true ]]; then
        git -C "$repository_root" commit -m "Release updated public mods" -m 'Release-Version-Bump: true'
        git -C "$repository_root" push origin HEAD:main
    fi
fi

mkdir -p -- "$output"
declare BepInExPath ValheimGamePath
while IFS='=' read -r key value; do
    case "$key" in
        BepInExPath) BepInExPath="$value" ;;
        ValheimGamePath) ValheimGamePath="$value" ;;
    esac
done < <("$dependency_script")
export BepInExPath ValheimGamePath
export FrameworkPathOverride="${FRAMEWORK_PATH_OVERRIDE:-/usr/lib/mono/4.8-api}"
[[ -f "$FrameworkPathOverride/mscorlib.dll" ]] || {
    echo "The .NET Framework 4.8 reference assemblies are unavailable. Install mono-devel." >&2
    exit 1
}

for index in "${!mods[@]}"; do
    mod="${mods[$index]}"
    directory="${directories[$index]}"
    dotnet build "$directory/Landoria.$mod.csproj" -c Release --nologo \
        -p:BepInExPath="$BepInExPath" -p:ValheimGamePath="$ValheimGamePath"
    commit="$(git -C "$repository_root" log -1 --format=%h --invert-grep \
        --grep='^Release-Version-Bump: true$' -- "Landoria.$mod")"
    archive="$(create_archive "$mod" "$directory" "$commit")"
    upload_archive "$directory" "$archive"
    version="$(jq -r '.version_number' "$directory/manifest.json")"
    echo "Uploaded Landoria-$mod-$version as a draft to the private package repository."
done
