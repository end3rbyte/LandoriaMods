#!/usr/bin/env bash
set -euo pipefail

[[ $# -eq 1 && -f "$1" ]] || { echo "Usage: $0 PLAN_FILE" >&2; exit 2; }
readonly plan_file="$1"
repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
readonly repository_root
: "${LANDORIA_MOD_REPOSITORY_URL:?LANDORIA_MOD_REPOSITORY_URL is required}"
readonly api_url="${LANDORIA_MOD_REPOSITORY_URL%/}"
readonly upstream_url="${api_url%/packages}/upstream/packages"
readonly secret_environment="${LANDORIA_MOD_REPOSITORY_SECRET_ENVIRONMENT:-/var/lib/landoria-secrets/mod-repository-upload.env}"
[[ -r "$secret_environment" ]] || { echo "The repository API environment is unavailable." >&2; exit 1; }
api_key="$(sed -n 's/^Authentication__ApiKey=//p' "$secret_environment")"
readonly api_key
[[ -n "$api_key" ]] || { echo "Authentication__ApiKey is missing." >&2; exit 1; }

delete_draft() {
    local package="$1" draft="$2" restored="$3" category="$4"
    local directory state released current status upstream_state
    directory="$repository_root/Landoria.$package"
    current="$(jq -er '.version_number' "$directory/manifest.json")"
    [[ "$current" == "$restored" ]] || {
        echo "$package source is $current; expected restored version $restored." >&2
        return 1
    }
    state="$(curl --fail --silent --show-error "$api_url/Landoria/$package")"
    released="$(jq -r --arg version "$draft" \
        '[.[] | select(.versionNumber == $version)] |
         if length == 0 then "missing" else (.[0].released | tostring) end' <<< "$state")"
    if [[ "$released" == missing ]]; then
        echo "Unused private draft $package $draft is already absent."
        return
    fi
    [[ "$released" == false ]] || {
        echo "$package $draft is released or unavailable and cannot be deleted." >&2
        return 1
    }
    if [[ "$category" != server-only ]]; then
        upstream_state="$(curl --fail --silent --show-error "$upstream_url/Landoria/$package")"
        ! jq -e --arg version "$draft" '.versions | any(.version_number == $version)' \
            <<< "$upstream_state" >/dev/null || {
            echo "$package $draft exists on Thunderstore and cannot be treated as an unused draft." >&2
            return 1
        }
    fi
    status="$(printf 'header = "X-Api-Key: %s"\n' "$api_key" | curl --silent --show-error \
        --output /dev/null --write-out '%{http_code}' --config - --request DELETE \
        "$api_url/Landoria/$package/$draft")"
    [[ "$status" == 204 ]] || { echo "Deleting $package $draft returned HTTP $status." >&2; return 1; }
    echo "Deleted unused private draft $package $draft."
}

jq -e '.schemaVersion == 1 and (.packages | type == "array" and length > 0)' \
    "$plan_file" >/dev/null || { echo "The rollback plan is invalid." >&2; exit 1; }
while IFS=$'\t' read -r package draft restored category; do
    delete_draft "$package" "$draft" "$restored" "$category"
done < <(jq -r '.packages[] | [.package,.draftVersion,.restoredVersion,.category] | @tsv' "$plan_file")
