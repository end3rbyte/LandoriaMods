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
    configured_plugins "$1"
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

argument_fixture='{"server":{"common":{"arguments":["--flycommand","true","--freeflycommand","true"],"mods":{"plugins":["Landoria.FreeFlyCommand"],"config":{"ModSentry_Required":["Landoria.FreeFlyCommand"]}}},"hammer":{"mods":{"plugins":["Landoria.FlyCommand"]}},"normal":{"mods":{}}}}'
[[ "$(configured_plugins "$argument_fixture")" == $'Landoria.FlyCommand\nLandoria.FreeFlyCommand' ]]
[[ "$(mod_paths "$argument_fixture" Landoria.FlyCommand)" == \
    'server/hammer/mods/plugins/Landoria.FlyCommand.dll' ]]
[[ -z "$(mod_paths "$argument_fixture" --flycommand)" ]]

fixture='{"server":{"common":{"items":{"Wood":10}},"hammer":{"items":{"Hammer":1,"Wood":20}},"normal":{}}}'
[[ "$(configured_items "$fixture" common)" == '{"Wood":10}' ]]
[[ "$(configured_items "$fixture" hammer)" == '{"Wood":20,"Hammer":1}' ]]
if configured_items "$fixture" normal >/dev/null; then
    echo "Normal unexpectedly produced a character template." >&2
    exit 1
fi

staging_directory="$(mktemp -d)"
trap 'find "$staging_directory" -depth -delete' EXIT
production_fixture='{"server":{"common":{"items":{"Wood":10},"mods":{"plugins":["Landoria.ModSentry"],"config":{"ModSentry_Optional":["Landoria.QuickLaunch"],"ModSentry_Required":["Landoria.ModSentry"]}}},"hammer":{"items":{"Hammer":1},"mods":{}},"normal":{"mods":{}}}}'
plan_expected_paths prod "$production_fixture"
expected_paths="$(cat "$staging_directory/expected-paths.txt")"
[[ "$expected_paths" == *'prod/server/common/mods/plugins/Landoria.ModSentry.dll'* ]]
[[ "$expected_paths" == *'prod/server/common/mods/config/ModSentry_Optional/Landoria.QuickLaunch.dll'* ]]
[[ "$expected_paths" == *'prod/server/common/mods/config/ModSentry_Required/Landoria.ModSentry.dll'* ]]
[[ "$expected_paths" == *'prod/server/common/mods/config/CharacterTemplate.yml'* ]]
[[ "$expected_paths" == *'prod/server/hammer/mods/config/CharacterTemplate.yml'* ]]
[[ "$expected_paths" != *'AzuAnticheat.dll'* ]]
[[ "$expected_paths" != *'prod/server/normal/mods/config/CharacterTemplate.yml'* ]]

LANDORIA_TEST_GAMEMODES_JSON='{"server":{"common":{"mods":{"plugins":["ServerCharacters"],"config":{"ModSentry_Required":["ServerCharacters"]}}}}}'
LANDORIA_PROD_GAMEMODES_JSON="$LANDORIA_TEST_GAMEMODES_JSON"
operations_file="$staging_directory/operations.tsv"
: > "$operations_file"
plan_production_external_policies
[[ "$(cat "$operations_file")" == $'copy\ttest/server/common/mods/config/ModSentry_Required/ServerCharacters.dll\tprod/server/common/mods/config/ModSentry_Required/ServerCharacters.dll\ncopy\ttest/server/common/mods/config/ModSentry_Required/ServerCharacters.dll\tprod/server/common/mods/plugins/ServerCharacters.dll' ]]

echo "Swiss Backup DLL synchronization configuration is valid."
