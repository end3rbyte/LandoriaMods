#!/usr/bin/env bash
set -euo pipefail

script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck disable=SC1091
. "$script_directory/character-template.sh"

gamemodes_json="${LANDORIA_GAMEMODES_JSON:-${LANDORIA_TEST_GAMEMODES_JSON:-}}"
storage_environment="${LANDORIA_STORAGE_ENVIRONMENT:-test}"
landoria_package_source="${LANDORIA_LANDORIA_PACKAGE_SOURCE:-private}"
[[ -n "$gamemodes_json" ]] || {
    echo "LANDORIA_GAMEMODES_JSON is required." >&2
    exit 2
}
[[ "$storage_environment" == test || "$storage_environment" == prod ]] || {
    echo "LANDORIA_STORAGE_ENVIRONMENT must be test or prod." >&2
    exit 2
}
[[ "$landoria_package_source" == private || "$landoria_package_source" == thunderstore ]] || {
    echo "LANDORIA_LANDORIA_PACKAGE_SOURCE must be private or thunderstore." >&2
    exit 2
}
storage_base_url="${LANDORIA_STORAGE_BASE_URL:-}"
storage_root="${LANDORIA_STORAGE_ROOT:-}"
if [[ -n "$storage_root" ]]; then
    [[ -d "$storage_root" ]] || {
        echo "LANDORIA_STORAGE_ROOT must reference a readable storage snapshot." >&2
        exit 2
    }
elif [[ ! "$storage_base_url" =~ ^https://[^[:space:]]+$ ]]; then
    echo "LANDORIA_STORAGE_BASE_URL or LANDORIA_STORAGE_ROOT must be configured." >&2
    exit 2
fi

repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
modpack_manifest="${LANDORIA_MODPACK_MANIFEST_PATH:-$repository_root/Landoria.LandoriaModPack/manifest.json}"
[[ -f "$modpack_manifest" ]] || {
    echo "The modpack manifest is unavailable: $modpack_manifest" >&2
    exit 2
}
: "${LANDORIA_MOD_REPOSITORY_URL:?LANDORIA_MOD_REPOSITORY_URL is required}"
private_repository_url="${LANDORIA_MOD_REPOSITORY_URL%/}"
temporary_directory="$(mktemp -d)"
trap 'rm -rf -- "$temporary_directory"' EXIT
version_reader="$repository_root/scripts/ci/DllMetadataVersion/DllMetadataVersion.csproj"
storage_request_id="${LANDORIA_STORAGE_REQUEST_ID:-$(date +%s%N)}"

for command in curl diff dotnet jq sha256sum sort unzip; do
    command -v "$command" >/dev/null 2>&1 || {
        echo "Required command not found: $command" >&2
        exit 1
    }
done

normalize() {
    tr '[:upper:]' '[:lower:]' <<< "$1" | tr -cd '[:alnum:]'
}

storage_url() {
    printf '%s/%s?request=%s\n' \
        "${storage_base_url%/}" "${1#/}" "$storage_request_id"
}

download_storage_file() {
    local relative_path="${1#/}" output="$2"
    [[ "$relative_path" != *'..'* ]] || {
        echo "The storage path is invalid: $relative_path" >&2
        return 1
    }
    if [[ -n "$storage_root" ]]; then
        cp -- "${storage_root%/}/$relative_path" "$output"
    else
        curl --fail --silent --show-error --location \
            "$(storage_url "$relative_path")" --output "$output"
    fi
}

version_matches() {
    [[ "$1" == "$2" || "$1" == "$2.0" ]]
}

dll_version() {
    dotnet run --configuration Release --no-restore \
        --project "$version_reader" -- "$1"
}

dotnet restore "$version_reader" >/dev/null

expected_paths="$temporary_directory/expected-paths.tsv"
jq -r '
    [.server | to_entries[] |
        .key as $variant |
        ((.value.mods.plugins // [])[] |
            ["server/\($variant)/mods/plugins/\(.).dll", "\(.).dll"]),
        ((.value.mods.config // {}) | to_entries[] |
            select(.key != "ModSentry_OptionalPolicy") |
            .key as $directory |
            .value[] |
            ["server/\($variant)/mods/config/\($directory)/\(.).dll", "\(.).dll"])] |
    .[] | @tsv
' <<< "$gamemodes_json" | sort -u > "$expected_paths"
[[ -s "$expected_paths" ]] || {
    echo "No $storage_environment DLL path was derived from GAMEMODES.yml." >&2
    exit 1
}

actual_files="$temporary_directory/actual-files.tsv"
: > "$actual_files"
for variant in common hammer normal; do
    manifest="$temporary_directory/$variant-manifest.json"
    download_storage_file "server/$variant/mods/manifest.json" "$manifest"
    jq -r --arg variant "$variant" '
        .files[] |
        select(.name | endswith(".dll")) |
        [
          ("server/" + $variant + "/mods" + .url),
          .name,
          .sha256
        ] | @tsv
    ' "$manifest" >> "$actual_files"
done
sort -u -o "$actual_files" "$actual_files"

cut -f1 "$expected_paths" > "$temporary_directory/expected-path-list"
cut -f1 "$actual_files" > "$temporary_directory/actual-path-list"
if ! diff -u "$temporary_directory/expected-path-list" \
    "$temporary_directory/actual-path-list"; then
    echo "The $storage_environment Swiss Backup DLL set does not exactly match GAMEMODES.yml." >&2
    exit 1
fi

while IFS= read -r dll; do
    [[ -n "$dll" ]] || continue
    hashes="$(awk -F '\t' -v dll="$dll" '$2 == dll { print toupper($3) }' \
        "$actual_files" | sort -u)"
    [[ "$(wc -l <<< "$hashes")" -eq 1 ]] || {
        echo "The $storage_environment copies of $dll do not have the same SHA-256 hash." >&2
        exit 1
    }
done < <(cut -f2 "$expected_paths" | sort -u)

dependency_for_dll() {
    local dll="$1" logical target dependency namespace package version candidate
    logical="${dll%.dll}"
    target="$logical"
    [[ "$target" == Landoria.* ]] && target="${target#Landoria.}"
    target="$(normalize "$target")"
    while IFS= read -r dependency; do
        IFS=$'\t' read -r namespace package version < <(
            jq -rn --arg dependency "$dependency" '
                $dependency |
                capture("^(?<namespace>[^-]+)-(?<package>.+)-(?<version>[0-9]+\\.[0-9]+\\.[0-9]+)$") |
                [.namespace, .package, .version] | @tsv
            '
        )
        candidate="$(normalize "$package")"
        if [[ "$candidate" == "$target" ]]; then
            printf '%s\t%s\t%s\n' "$namespace" "$package" "$version"
            return 0
        fi
    done < <(jq -r '.dependencies[]' "$modpack_manifest")
    return 1
}

verify_server_only_copies() {
    local dll="$1" stored_dll stored_hash stored_path stored_version actual_hash
    local consistent_hash=''
    while IFS=$'\t' read -r stored_path _ stored_hash; do
        [[ -n "$stored_path" ]] || continue
        stored_dll="$temporary_directory/server-only-$(normalize "$stored_path").dll"
        download_storage_file "$stored_path" "$stored_dll"
        actual_hash="$(sha256sum "$stored_dll" | awk '{print toupper($1)}')"
        [[ "$actual_hash" == "${stored_hash^^}" ]] || {
            echo "$stored_path does not match its published SHA-256 hash." >&2
            return 1
        }
        consistent_hash="${stored_hash^^}"
        stored_version="$(dll_version "$stored_dll")"
    done < <(awk -F '\t' -v dll="$dll" '$2 == dll' "$actual_files")
    echo "Verified server-only $dll FileVersion $stored_version and consistent SHA-256 $consistent_hash."
}

verify_package_dll() {
    local dll="$1" namespace package version dependency archive manifest_version
    local target entry entries expected_hash package_hash package_dll package_version
    local stored_dll stored_hash stored_path stored_version
    dependency="$(dependency_for_dll "$dll")"
    IFS=$'\t' read -r namespace package version <<< "$dependency"

    archive="$temporary_directory/$namespace-$package-$version.zip"
    curl --fail --silent --show-error --location \
        "$private_repository_url/$namespace/$package/$version/download" \
        --output "$archive"
    manifest_version="$(unzip -p "$archive" manifest.json | jq -er '.version_number')"
    [[ "$manifest_version" == "$version" ]] || {
        echo "$namespace-$package archive manifest declares $manifest_version; expected $version." >&2
        return 1
    }
    target="$(normalize "${dll%.dll}")"
    entries="$({ unzip -Z1 "$archive" | while IFS= read -r entry; do
        [[ "$entry" == *.dll ]] || continue
        [[ "$(normalize "$(basename "$entry" .dll)")" == "$target" ]] && printf '%s\n' "$entry"
    done; } || true)"
    [[ -n "$entries" && "$(wc -l <<< "$entries")" -eq 1 ]] || {
        echo "Expected exactly one $dll in $namespace-$package-$version." >&2
        return 1
    }
    entry="$entries"
    package_dll="$temporary_directory/package-$dll"
    unzip -p "$archive" "$entry" > "$package_dll"
    package_version="$(dll_version "$package_dll")"
    if [[ "$namespace" == Landoria ]]; then
        version_matches "$package_version" "$manifest_version" || {
            echo "$dll has FileVersion $package_version, but its manifest declares $manifest_version." >&2
            return 1
        }
    fi
    package_hash="$(sha256sum "$package_dll" | awk '{print toupper($1)}')"
    expected_hash="$(awk -F '\t' -v dll="$dll" '$2 == dll { print toupper($3); exit }' \
        "$actual_files")"

    while IFS=$'\t' read -r stored_path _ stored_hash; do
        [[ -n "$stored_path" ]] || continue
        stored_dll="$temporary_directory/stored-$(normalize "$stored_path").dll"
        download_storage_file "$stored_path" "$stored_dll"
        stored_version="$(dll_version "$stored_dll")"
        [[ "$stored_version" == "$package_version" ]] || {
            echo "$stored_path has FileVersion $stored_version; expected $package_version." >&2
            return 1
        }
        [[ "$(sha256sum "$stored_dll" | awk '{print toupper($1)}')" == "${stored_hash^^}" ]] || {
            echo "$stored_path does not match its published SHA-256 hash." >&2
            return 1
        }
        [[ "${stored_hash^^}" == "$package_hash" ]] || {
            echo "$stored_path does not match $namespace-$package-$manifest_version." >&2
            echo "Expected SHA-256 $package_hash, found ${stored_hash^^}." >&2
            return 1
        }
    done < <(awk -F '\t' -v dll="$dll" '$2 == dll' "$actual_files")

    [[ "$package_hash" == "$expected_hash" ]] || {
        echo "$dll has inconsistent hashes in the test manifests." >&2
        return 1
    }

    echo "Verified $dll FileVersion $package_version and SHA-256 $package_hash."
}

while IFS= read -r dll; do
    [[ -n "$dll" ]] || continue
    if dependency_for_dll "$dll" >/dev/null; then
        verify_package_dll "$dll"
    else
        verify_server_only_copies "$dll"
    fi
done < <(cut -f2 "$expected_paths" | sort -u)

verify_character_templates() {
    local variant items expected stored manifest_hash stored_hash
    for variant in common hammer normal; do
        manifest_hash="$(jq -r '
            first(.files[] | select(.url == "/config/CharacterTemplate.yml") | .sha256) // empty
        ' "$temporary_directory/$variant-manifest.json")"
        if ! items="$(configured_items "$gamemodes_json" "$variant")"; then
            [[ -z "$manifest_hash" ]] || {
                echo "Unexpected CharacterTemplate.yml in $storage_environment/$variant." >&2
                return 1
            }
            continue
        fi
        [[ -n "$manifest_hash" ]] || {
            echo "CharacterTemplate.yml is missing from $storage_environment/$variant." >&2
            return 1
        }
        expected="$temporary_directory/$variant-expected-CharacterTemplate.yml"
        stored="$temporary_directory/$variant-stored-CharacterTemplate.yml"
        render_character_template "$items" "$expected"
        download_storage_file "server/$variant/mods/config/CharacterTemplate.yml" "$stored"
        stored_hash="$(sha256sum "$stored" | awk '{print toupper($1)}')"
        [[ "$stored_hash" == "${manifest_hash^^}" ]] || {
            echo "$storage_environment/$variant CharacterTemplate.yml does not match its manifest." >&2
            return 1
        }
        cmp --silent "$expected" "$stored" || {
            echo "$storage_environment/$variant CharacterTemplate.yml does not match GAMEMODES.yml." >&2
            return 1
        }
    done
}

verify_character_templates

echo "The $storage_environment Swiss Backup DLL set, versions, and hashes match the modpack and GAMEMODES.yml."
