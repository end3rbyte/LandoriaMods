#!/usr/bin/env bash
set -euo pipefail

[[ -n "${LANDORIA_TEST_GAMEMODES_JSON:-}" ]] || {
    echo "LANDORIA_TEST_GAMEMODES_JSON is required." >&2
    exit 2
}
[[ "${LANDORIA_STORAGE_BASE_URL:-}" =~ ^https://[^[:space:]]+$ ]] || {
    echo "LANDORIA_STORAGE_BASE_URL must be an absolute HTTPS URL." >&2
    exit 2
}

repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
modpack_manifest="$repository_root/Landoria.LandoriaModPack/manifest.json"
private_repository_url="${LANDORIA_MOD_REPOSITORY_URL:-https://test.landoria-gaming.com:8443/api/v1/packages}"
temporary_directory="$(mktemp -d)"
trap 'rm -rf -- "$temporary_directory"' EXIT
version_reader="$repository_root/scripts/ci/DllMetadataVersion/DllMetadataVersion.csproj"

for command in curl diff dotnet jq sha256sum sort unzip; do
    command -v "$command" >/dev/null 2>&1 || {
        echo "Required command not found: $command" >&2
        exit 1
    }
done

normalize() {
    tr '[:upper:]' '[:lower:]' <<< "$1" | tr -cd '[:alnum:]'
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
    paths(scalars) as $path |
    getpath($path) as $plugin |
    select($plugin | type == "string") |
    select($path | length >= 5) |
    select($path[-1] | type == "number") |
    [($path[0:-1] + [($plugin + ".dll")] | join("/")), ($plugin + ".dll")] |
    @tsv
' <<< "$LANDORIA_TEST_GAMEMODES_JSON" | sort -u > "$expected_paths"
[[ -s "$expected_paths" ]] || {
    echo "No test DLL path was derived from GAMEMODES.yml." >&2
    exit 1
}

actual_files="$temporary_directory/actual-files.tsv"
: > "$actual_files"
for variant in common hammer normal; do
    manifest="$temporary_directory/$variant-manifest.json"
    curl --fail --silent --show-error --location \
        "$LANDORIA_STORAGE_BASE_URL/server/$variant/mods/manifest.json" \
        --output "$manifest"
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
    echo "The test Swiss Backup DLL set does not exactly match GAMEMODES.yml." >&2
    exit 1
fi

while IFS= read -r dll; do
    [[ -n "$dll" ]] || continue
    hashes="$(awk -F '\t' -v dll="$dll" '$2 == dll { print toupper($3) }' \
        "$actual_files" | sort -u)"
    [[ "$(wc -l <<< "$hashes")" -eq 1 ]] || {
        echo "The test copies of $dll do not have the same SHA-256 hash." >&2
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

latest_private_version() {
    curl --fail --silent --show-error --location \
        "$private_repository_url/Landoria/$1" |
        jq -er 'sort_by(.versionNumber | split(".") | map(tonumber)) | last.versionNumber'
}

verify_package_dll() {
    local dll="$1" namespace package version dependency archive manifest_version
    local target entry entries expected_hash package_hash package_dll package_version
    local stored_dll stored_hash stored_path stored_url stored_version
    if dependency="$(dependency_for_dll "$dll")"; then
        IFS=$'\t' read -r namespace package version <<< "$dependency"
    elif [[ "$dll" == Landoria.*.dll ]]; then
        namespace=Landoria
        package="${dll#Landoria.}"
        package="${package%.dll}"
        version="$(latest_private_version "$package")"
    else
        echo "$dll is neither a modpack dependency nor a Landoria server package." >&2
        return 1
    fi

    archive="$temporary_directory/$namespace-$package-$version.zip"
    if [[ "$namespace" == Landoria ]]; then
        curl --fail --silent --show-error --location \
            "$private_repository_url/$namespace/$package/$version/download" \
            --output "$archive"
    else
        curl --fail --silent --show-error --location \
            "https://thunderstore.io/package/download/$namespace/$package/$version/" \
            --output "$archive"
    fi
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
        stored_url="${LANDORIA_STORAGE_BASE_URL%/}/$stored_path"
        curl --fail --silent --show-error --location "$stored_url" --output "$stored_dll"
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
    [[ -n "$dll" ]] && verify_package_dll "$dll"
done < <(cut -f2 "$expected_paths" | sort -u)

echo "The test Swiss Backup DLL set, versions, and hashes match the modpack and GAMEMODES.yml."
