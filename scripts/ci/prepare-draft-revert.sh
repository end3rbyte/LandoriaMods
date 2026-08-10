#!/usr/bin/env bash
set -euo pipefail

[[ $# -ge 2 ]] || { echo "Usage: $0 PLAN_ID MOD [MOD ...]" >&2; exit 2; }
readonly plan_id="$1"
shift
[[ "$plan_id" =~ ^[A-Za-z0-9._-]+$ ]] || { echo "The plan ID is invalid." >&2; exit 2; }

repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
readonly repository_root
: "${LANDORIA_MOD_REPOSITORY_URL:?LANDORIA_MOD_REPOSITORY_URL is required}"
readonly api_url="${LANDORIA_MOD_REPOSITORY_URL%/}"
readonly upstream_url="${api_url%/packages}/upstream/packages"
readonly configuration="${LANDORIA_MODPACK_CONFIGURATION_JSON:?LANDORIA_MODPACK_CONFIGURATION_JSON is required}"
readonly plan_directory="$repository_root/.draft-rollbacks"
readonly plan_file="$plan_directory/$plan_id.json"
declare -a plan_entries=()
modpack_required=false

latest_private_draft() {
    curl --fail --silent --show-error --retry 3 --retry-all-errors \
        "$api_url/Landoria/$1" | jq -er \
        '[.[] | select(.released == false)] |
         if length == 1 then .[0].versionNumber
         elif length == 0 then error("no draft exists")
         else error("multiple drafts exist") end'
}

category_for() {
    [[ "$1" == LandoriaModPack ]] && { echo modpack; return; }
    jq -er --arg package "$1" '
        if .landoria_packages | index($package) then "modpack"
        elif .server_only_packages | index($package) then "server-only"
        elif .standalone_packages | index($package) then "standalone"
        else error("package is not in the central inventory") end
    ' <<< "$configuration"
}

assert_empty_draft() {
    local mod="$1" package="$2" category="$3" restored="$4" tag changed file line
    if [[ "$category" == server-only ]]; then
        tag="production/server-only/$package/$restored"
    else
        tag="thunderstore/$mod/$restored"
    fi
    git -C "$repository_root" rev-parse --verify --quiet "refs/tags/$tag" >/dev/null || {
        echo "The release tag is unavailable: $tag" >&2
        return 1
    }
    changed="$(git -C "$repository_root" diff --name-only "$tag" -- "Landoria.$mod")"
    while IFS= read -r file; do
        [[ -n "$file" ]] || continue
        case "$file" in
            "Landoria.$mod/"*Plugin.cs|\
            "Landoria.$mod/Properties/AssemblyInfo.cs"|\
            "Landoria.$mod/manifest.json") ;;
            *)
                echo "$package draft contains a real change relative to $tag: $file" >&2
                return 1
                ;;
        esac
    done <<< "$changed"
    while IFS= read -r line; do
        [[ "$line" == +* || "$line" == -* ]] || continue
        [[ "$line" != +++* && "$line" != ---* ]] || continue
        case "$line" in
            *PluginVersion*|*AssemblyVersion*|*AssemblyFileVersion*|*'"version_number"'*) ;;
            *)
                if [[ "$package" == LandoriaModPack &&
                      "$line" =~ ^[+-].*Landoria-[A-Za-z0-9_]+-[0-9]+\.[0-9]+\.[0-9]+ ]]; then
                    continue
                fi
                echo "$package draft contains a non-version change relative to $tag: $line" >&2
                return 1
                ;;
        esac
    done < <(git -C "$repository_root" diff --unified=0 "$tag" -- "Landoria.$mod")
}

released_version() {
    local package="$1" category="$2"
    if [[ "$category" == server-only ]]; then
        git -C "$repository_root" tag --list "production/server-only/$package/*" |
            sed "s#production/server-only/$package/##" | sort -V | tail -n 1
        return
    fi
    curl --fail --silent --show-error --retry 3 --retry-all-errors \
        "$upstream_url/Landoria/$package" |
        jq -er '.latest.version_number'
}

set_source_version() {
    local mod="$1" version="$2" directory plugin temporary
    directory="$repository_root/Landoria.$mod"
    plugin="$(find "$directory" -maxdepth 1 -type f -name '*Plugin.cs' -print -quit)"
    [[ -n "$plugin" && -f "$directory/Properties/AssemblyInfo.cs" ]] || {
        echo "Versioned source files are unavailable for $mod." >&2
        return 1
    }
    perl -0pi -e "s/(PluginVersion\\s*=\\s*\")\\d+\\.\\d+\\.\\d+(\";)/\${1}$version\${2}/" "$plugin"
    perl -0pi -e "s/(AssemblyVersion\\(\")\\d+\\.\\d+\\.\\d+(\\.\\*\"\\))/\${1}$version\${2}/; s/(AssemblyFileVersion\\(\")\\d+\\.\\d+\\.\\d+(\"\\))/\${1}$version\${2}/" \
        "$directory/Properties/AssemblyInfo.cs"
    temporary="$directory/manifest.json.tmp"
    jq --arg version "$version" '.version_number = $version' \
        "$directory/manifest.json" > "$temporary"
    mv -- "$temporary" "$directory/manifest.json"
}

restore_modpack_dependencies() {
    local version="$1" manifest="$repository_root/Landoria.LandoriaModPack/manifest.json"
    local dependencies temporary
    dependencies="$(git -C "$repository_root" show \
        "thunderstore/LandoriaModPack/$version:Landoria.LandoriaModPack/manifest.json" |
        jq -c '.dependencies')"
    temporary="$manifest.tmp"
    jq --argjson dependencies "$dependencies" '.dependencies = $dependencies' \
        "$manifest" > "$temporary"
    mv -- "$temporary" "$manifest"
}

add_plan_entry() {
    plan_entries+=("$(jq -cn --arg package "$1" --arg draft "$2" \
        --arg restored "$3" --arg category "$4" \
        '{package:$package,draftVersion:$draft,restoredVersion:$restored,category:$category}')")
}

prepare_package() {
    local mod="$1" directory package current draft category restored
    [[ "$mod" =~ ^[A-Za-z0-9_]+$ ]] || { echo "Invalid mod name: $mod" >&2; return 1; }
    directory="$repository_root/Landoria.$mod"
    [[ -f "$directory/manifest.json" ]] || { echo "Unknown mod: $mod" >&2; return 1; }
    package="$(jq -er '.name' "$directory/manifest.json")"
    current="$(jq -er '.version_number' "$directory/manifest.json")"
    draft="$(latest_private_draft "$package")"
    [[ "$current" == "$draft" ]] || {
        echo "Local $package version $current does not match private draft $draft." >&2
        return 1
    }
    category="$(category_for "$package")"
    restored="$(released_version "$package" "$category")"
    [[ "$restored" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ && "$restored" != "$draft" ]] || {
        echo "No earlier released version is available for $package." >&2
        return 1
    }
    assert_empty_draft "$mod" "$package" "$category" "$restored"
    set_source_version "$mod" "$restored"
    [[ "$package" != LandoriaModPack ]] || restore_modpack_dependencies "$restored"
    add_plan_entry "$package" "$draft" "$restored" "$category"
    jq -e --arg package "$package" '.landoria_packages | index($package) != null' \
        <<< "$configuration" >/dev/null && modpack_required=true
    echo "Prepared $package draft $draft for rollback to $restored."
}

for mod in "$@"; do prepare_package "$mod"; done

if [[ "$modpack_required" == true && " $* " != *" LandoriaModPack "* ]]; then
    prepare_package LandoriaModPack
fi

for entry in "${plan_entries[@]}"; do
    package="$(jq -r '.package' <<< "$entry")"
    restored="$(jq -r '.restoredVersion' <<< "$entry")"
    [[ "$package" == LandoriaModPack ]] && continue
    jq --arg prefix "Landoria-$package-" --arg dependency "Landoria-$package-$restored" \
        '.dependencies |= map(if startswith($prefix) then $dependency else . end)' \
        "$repository_root/Landoria.LandoriaModPack/manifest.json" > \
        "$repository_root/Landoria.LandoriaModPack/manifest.json.tmp"
    mv -- "$repository_root/Landoria.LandoriaModPack/manifest.json.tmp" \
        "$repository_root/Landoria.LandoriaModPack/manifest.json"
done

mkdir -p "$plan_directory"
printf '%s\n' "${plan_entries[@]}" | jq -s \
    --arg createdAt "$(date --utc +'%Y-%m-%dT%H:%M:%SZ')" \
    '{schemaVersion:1,createdAt:$createdAt,packages:.}' > "$plan_file"
echo "Created rollback plan ${plan_file#"$repository_root/"}."
