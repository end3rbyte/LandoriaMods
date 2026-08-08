#!/usr/bin/env bash
set -euo pipefail

[[ -n "${LANDORIA_TEST_GAMEMODES_JSON:-}" && \
   -n "${LANDORIA_PROD_GAMEMODES_JSON:-}" ]] || {
    echo "Both test and production game-mode configurations are required." >&2
    exit 2
}

repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
# shellcheck disable=SC1091
source "$repository_root/scripts/ci/sync-swiss-backup-dlls.sh"

plugins() {
    jq -r '.. | strings | select(startswith("Landoria."))' <<< "$1" | sort -u
}

validate_configuration_paths() {
    local configuration="$1" plugin path
    while IFS= read -r plugin; do
        [[ -n "$plugin" ]] || continue
        while IFS= read -r path; do
            validate_relative_path "$path" || {
                echo "Invalid generated path for $plugin: $path" >&2
                exit 1
            }
        done < <(mod_paths "$configuration" "$plugin")
    done < <(plugins "$configuration")
}

validate_configuration_paths "$LANDORIA_TEST_GAMEMODES_JSON"
validate_configuration_paths "$LANDORIA_PROD_GAMEMODES_JSON"

echo "Swiss Backup DLL synchronization configuration is valid."
