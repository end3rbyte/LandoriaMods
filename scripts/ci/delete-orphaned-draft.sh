#!/usr/bin/env bash
set -euo pipefail

[[ $# -eq 2 ]] || { echo "Usage: $0 PACKAGE VERSION" >&2; exit 2; }
readonly package="$1"
readonly version="$2"
[[ "$package" =~ ^[A-Za-z0-9_]+$ ]] || { echo "The package name is invalid." >&2; exit 2; }
[[ "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]] || { echo "The version is invalid." >&2; exit 2; }

: "${LANDORIA_MOD_REPOSITORY_URL:?LANDORIA_MOD_REPOSITORY_URL is required}"
: "${LANDORIA_MODPACK_CONFIGURATION_JSON:?LANDORIA_MODPACK_CONFIGURATION_JSON is required}"
readonly api_url="${LANDORIA_MOD_REPOSITORY_URL%/}"
readonly upstream_url="${api_url%/packages}/upstream/packages"
readonly secret_environment="${LANDORIA_MOD_REPOSITORY_SECRET_ENVIRONMENT:-/var/lib/landoria-secrets/mod-repository-upload.env}"

jq -e --arg package "$package" '
    ([.landoria_packages[], .server_only_packages[], .standalone_packages[]] |
     index($package)) == null
' <<< "$LANDORIA_MODPACK_CONFIGURATION_JSON" >/dev/null || {
    echo "$package is still present in the central inventory." >&2
    exit 1
}

state="$(curl --fail --silent --show-error --retry 3 --retry-all-errors \
    "$api_url/Landoria/$package")"
jq -e --arg version "$version" '
    [.[] | select(.versionNumber == $version and .released == false)] | length == 1
' <<< "$state" >/dev/null || {
    echo "Exactly one matching private draft is required." >&2
    exit 1
}

upstream_status="$(curl --silent --show-error --output /dev/null --write-out '%{http_code}' \
    "$upstream_url/Landoria/$package")"
[[ "$upstream_status" == 404 ]] || {
    echo "$package exists upstream and cannot be deleted as an orphaned draft." >&2
    exit 1
}

[[ -r "$secret_environment" ]] || { echo "The repository API environment is unavailable." >&2; exit 1; }
api_key="$(sed -n 's/^Authentication__ApiKey=//p' "$secret_environment")"
[[ -n "$api_key" ]] || { echo "Authentication__ApiKey is missing." >&2; exit 1; }
status="$(printf 'header = "X-Api-Key: %s"\n' "$api_key" | curl --silent --show-error \
    --output /dev/null --write-out '%{http_code}' --config - --request DELETE \
    "$api_url/Landoria/$package/$version")"
[[ "$status" == 204 ]] || { echo "Deleting $package $version returned HTTP $status." >&2; exit 1; }
echo "Deleted orphaned private draft $package $version."
