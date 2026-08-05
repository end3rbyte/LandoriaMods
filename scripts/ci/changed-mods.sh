#!/usr/bin/env bash
set -euo pipefail

[[ $# -eq 2 ]] || { echo "Usage: $0 BEFORE AFTER" >&2; exit 2; }
repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
before="$1"
after="$2"

all_mods() {
    find "$repository_root" -mindepth 2 -maxdepth 2 -type f -name manifest.json \
        -path "$repository_root/Landoria.*/manifest.json" -printf '%h\n' |
        xargs -r -n 1 basename |
        sed -n 's/^Landoria\.//p' | grep -v '^SharedLib$' | sort
}

if [[ "$before" == "0000000000000000000000000000000000000000" ]]; then
    mapfile -t paths < <(git -C "$repository_root" show --pretty='' --name-only "$after")
else
    mapfile -t paths < <(git -C "$repository_root" diff --name-only "$before" "$after")
fi

publish_all=false
declare -A selected=()
for path in "${paths[@]}"; do
    case "$path" in
        Directory.Build.props|Landoria.SharedLib/*)
            publish_all=true
            ;;
        Landoria.*/*)
            project="${path%%/*}"
            mod="${project#Landoria.}"
            [[ "$mod" == SharedLib ]] || selected["$mod"]=1
            ;;
    esac
done

if [[ "$publish_all" == true ]]; then
    all_mods
else
    printf '%s\n' "${!selected[@]}" | sed '/^$/d' | sort
fi
