#!/usr/bin/env bash
set -euo pipefail

cache_root="${LANDORIA_MOD_BUILD_CACHE:-${HOME}/.cache/landoria-mod-build}"
steamcmd_root="$cache_root/steamcmd"
valheim_root="$cache_root/valheim-server"
bepinex_version="5.4.2333"
bepinex_install_root="$cache_root/bepinex-valheim-$bepinex_version"

require_command() {
    command -v "$1" >/dev/null 2>&1 || {
        echo "Required command not found: $1" >&2
        exit 1
    }
}

install_steamcmd() {
    [[ -x "$steamcmd_root/steamcmd.sh" ]] && return
    mkdir -p "$steamcmd_root"
    curl --fail --location --silent --show-error \
        https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz |
        tar -xz -C "$steamcmd_root"
}

install_bepinex() {
    [[ -f "$bepinex_install_root/BepInEx/core/BepInEx.dll" ]] && return
    local temporary
    temporary="$(mktemp -d)"
    curl --fail --location --silent --show-error \
        "https://thunderstore.io/package/download/denikson/BepInExPack_Valheim/${bepinex_version}/" \
        --output "$temporary/bepinex.zip"
    rm -rf -- "$bepinex_install_root"
    mkdir -p "$bepinex_install_root"
    unzip -q "$temporary/bepinex.zip" -d "$temporary/content"
    cp -a "$temporary/content/BepInExPack_Valheim/BepInEx" "$bepinex_install_root/"
    rm -rf -- "$temporary"
}

for command in curl tar unzip; do
    require_command "$command"
done

mkdir -p "$cache_root"
install_steamcmd
"$steamcmd_root/steamcmd.sh" \
    +@sSteamCmdForcePlatformType linux \
    +force_install_dir "$valheim_root" \
    +login anonymous \
    +app_update 896660 validate \
    +quit
if [[ ! -e "$valheim_root/valheim_Data" ]]; then
    ln -s valheim_server_Data "$valheim_root/valheim_Data"
fi
install_bepinex

[[ -f "$valheim_root/valheim_server_Data/Managed/assembly_valheim.dll" ]] || {
    echo "Valheim managed assemblies were not installed." >&2
    exit 1
}

printf 'BepInExPath=%s\n' "$bepinex_install_root/BepInEx"
printf 'ValheimGamePath=%s\n' "$valheim_root"
