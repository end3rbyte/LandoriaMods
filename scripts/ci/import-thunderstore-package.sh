#!/usr/bin/env bash
set -euo pipefail

[[ $# -eq 2 && "$1" =~ ^[A-Za-z0-9_]+$ && "$2" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]] || {
    echo "Usage: $0 PACKAGE VERSION" >&2
    exit 2
}

readonly package="$1"
readonly version="$2"
readonly namespace=Landoria
readonly api_url="${LANDORIA_MOD_REPOSITORY_URL:-https://test.landoria-gaming.com:8443/api/v1/packages}"
readonly secret_environment="${LANDORIA_MOD_REPOSITORY_SECRET_ENVIRONMENT:-/var/lib/landoria-secrets/mod-repository-upload.env}"
temporary_directory="$(mktemp -d)"
trap 'find "$temporary_directory" -depth -delete' EXIT
readonly archive="$temporary_directory/$namespace-$package-$version.zip"

for command in curl jq sha256sum unzip; do
    command -v "$command" >/dev/null 2>&1 || {
        echo "Required command not found: $command" >&2
        exit 1
    }
done

curl --fail --silent --show-error --location \
    "https://thunderstore.io/package/download/$namespace/$package/$version/" \
    --output "$archive"

manifest="$(unzip -p "$archive" manifest.json)"
jq -e --arg package "$package" --arg version "$version" '
    .name == $package and .version_number == $version and
    (.categories | type == "array" and length > 0)
' <<< "$manifest" >/dev/null || {
    echo "The downloaded Thunderstore package metadata is invalid." >&2
    exit 1
}

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
result="$(printf 'header = "X-Api-Key: %s"\n' "$api_key" | curl --fail \
    --silent --show-error --config - \
    --form "namespace=$namespace" --form "categories=$categories" \
    --form "package=@$archive;type=application/zip" "$api_url")"
jq -e --arg version "$version" \
    '.versionNumber == $version and .released == false' <<< "$result" >/dev/null || {
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
