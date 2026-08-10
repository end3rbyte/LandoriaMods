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

fixture='{"server":{"common":{"items":{"Wood":10}},"hammer":{"items":{"Hammer":1,"Wood":20}},"normal":{}}}'
[[ "$(configured_items "$fixture" common)" == '{"Wood":10}' ]]
[[ "$(configured_items "$fixture" hammer)" == '{"Wood":20,"Hammer":1}' ]]
if configured_items "$fixture" normal >/dev/null; then
    echo "Normal unexpectedly produced a character template." >&2
    exit 1
fi

echo "Swiss Backup DLL synchronization configuration is valid."
