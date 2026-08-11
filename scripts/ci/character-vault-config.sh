#!/usr/bin/env bash

configured_items() {
    local configuration="$1" variant="$2"
    jq -ce --arg variant "$variant" '
        .server as $server |
        ($server.common.items // {}) as $common |
        if $variant == "common" then
            ($common | select(length > 0))
        elif ($server[$variant] | has("items")) then
            ($common + $server[$variant].items)
        else
            empty
        end
    ' <<< "$configuration"
}

render_character_vault_config() {
    local items="$1" output="$2"
    {
        echo '# Managed by Landoria from GAMEMODES.yml.'
        echo '[New Characters]'
        echo
        printf 'StartingItems = '
        jq -jr 'to_entries | sort_by(.key) | map("\(.key):\(.value)") | join(",")' <<< "$items"
        echo
    } > "$output"
}
