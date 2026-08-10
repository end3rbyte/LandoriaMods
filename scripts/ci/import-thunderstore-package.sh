#!/usr/bin/env bash
set -euo pipefail

[[ $# -eq 4 && "$1" =~ ^[A-Za-z0-9_]+$ && "$2" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ && \
   "$3" =~ ^(thunderstore|test-storage)$ && "$4" =~ ^(true|false)$ ]] || {
    echo "Usage: $0 PACKAGE VERSION thunderstore|test-storage true|false" >&2
    exit 2
}

readonly package="$1"
readonly version="$2"
readonly source="$3"
readonly replace_existing="$4"
readonly namespace=Landoria
: "${LANDORIA_MOD_REPOSITORY_URL:?LANDORIA_MOD_REPOSITORY_URL is required}"
readonly api_url="${LANDORIA_MOD_REPOSITORY_URL%/}"
readonly secret_environment="${LANDORIA_MOD_REPOSITORY_SECRET_ENVIRONMENT:-/var/lib/landoria-secrets/mod-repository-upload.env}"
temporary_directory="$(mktemp -d)"
trap 'find "$temporary_directory" -depth -delete' EXIT
readonly archive="$temporary_directory/$namespace-$package-$version.zip"
repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
readonly repository_root

for command in curl dotnet jq sha256sum unzip zip; do
    command -v "$command" >/dev/null 2>&1 || {
        echo "Required command not found: $command" >&2
        exit 1
    }
done

if [[ "$source" == thunderstore ]]; then
    curl --fail --silent --show-error --location \
        "$api_url/$namespace/$package/$version/download" \
        --output "$archive"
else
    [[ -n "${LANDORIA_TEST_GAMEMODES_JSON:-}" && \
       "${LANDORIA_STORAGE_BASE_URL:-}" =~ ^https://[^[:space:]]+$ ]] || {
        echo "Test storage configuration is required." >&2
        exit 2
    }
    plugin="$namespace.$package"
    storage_path="$(jq -er --arg plugin "$plugin" '
        [paths(scalars) as $path |
            select(getpath($path) == $plugin) |
            ($path[0:-1] + [($plugin + ".dll")]) | join("/")] |
        unique | first
    ' <<< "$LANDORIA_TEST_GAMEMODES_JSON")"
    [[ "$storage_path" =~ ^server/(common|hammer|normal)/mods/(plugins|config/[A-Za-z0-9._-]+)/[A-Za-z0-9._-]+\.dll$ ]] || {
        echo "No valid test storage path was found for $plugin." >&2
        exit 1
    }
    staging="$temporary_directory/staging"
    mkdir -p "$staging"
    curl --fail --silent --show-error --location \
        "${LANDORIA_STORAGE_BASE_URL%/}/$storage_path" \
        --output "$staging/$plugin.dll"
    dotnet restore "$repository_root/scripts/ci/DllMetadataVersion/DllMetadataVersion.csproj" \
        >/dev/null
    dll_version="$(dotnet run --configuration Release --no-restore \
        --project "$repository_root/scripts/ci/DllMetadataVersion/DllMetadataVersion.csproj" \
        -- "$staging/$plugin.dll")"
    [[ "$dll_version" == "$version" || "$dll_version" == "$version.0" ]] || {
        echo "$plugin.dll has FileVersion $dll_version; expected $version." >&2
        exit 1
    }
    package_directory="$repository_root/$plugin"
    cp -- "$package_directory/icon.png" "$staging/icon.png"
    cp -- "$package_directory/README.Thunderstore.md" "$staging/README.md"
    cp -- "$package_directory/CHANGELOG.md" "$staging/CHANGELOG.md"
    jq --arg version "$version" '.version_number = $version' \
        "$package_directory/manifest.json" > "$staging/manifest.json"
    (cd "$staging" && zip -q "$archive" ./*)
fi

manifest="$(unzip -p "$archive" manifest.json)"
jq -e --arg package "$package" --arg version "$version" '
    .name == $package and .version_number == $version and
    (.categories | type == "array" and length > 0)
' <<< "$manifest" >/dev/null || {
    echo "The downloaded Thunderstore package metadata is invalid." >&2
    exit 1
}

if [[ "$source" == thunderstore ]]; then
    metadata="$(curl --fail --silent --show-error --location \
        "$api_url/$namespace/$package")"
    jq -e --arg version "$version" '
        any(.[]; .versionNumber == $version and .released == true and
            .source == "Thunderstore")
    ' <<< "$metadata" >/dev/null || {
        echo "The cached package is not an immutable Thunderstore release." >&2
        exit 1
    }
    echo "Verified cached Thunderstore release $namespace-$package-$version."
    exit 0
fi

[[ -r "$secret_environment" ]] || {
    echo "The Vault Agent API environment is unavailable: $secret_environment" >&2
    exit 1
}
api_key="$(sed -n 's/^Authentication__ApiKey=//p' "$secret_environment")"
[[ -n "$api_key" ]] || {
    echo "Authentication__ApiKey is missing." >&2
    exit 1
}
categories="$(jq -r '.categories | join(",")' <<< "$manifest")"
if [[ "$replace_existing" == true ]]; then
    upload_url="$api_url/$namespace/$package/$version"
    upload_method=PUT
else
    upload_url="$api_url"
    upload_method=POST
fi
result="$(printf 'header = "X-Api-Key: %s"\n' "$api_key" | curl --fail \
    --silent --show-error --config - --request "$upload_method" \
    --form "namespace=$namespace" --form "categories=$categories" \
    --form "package=@$archive;type=application/zip" "$upload_url")"
jq -e --arg version "$version" --argjson replaced "$replace_existing" '
    .versionNumber == $version and
    (if $replaced then .replaced == true else .released == false end)
' <<< "$result" >/dev/null || {
    echo "The private repository returned unexpected package metadata." >&2
    exit 1
}

remote_archive="$temporary_directory/private-$namespace-$package-$version.zip"
curl --fail --silent --show-error --location \
    "$api_url/$namespace/$package/$version/download" --output "$remote_archive"
[[ "$(sha256sum "$remote_archive" | awk '{print toupper($1)}')" == \
   "$(sha256sum "$archive" | awk '{print toupper($1)}')" ]] || {
    echo "The imported private package does not match the Thunderstore archive." >&2
    exit 1
}

echo "Imported $namespace-$package-$version into the private package repository."
