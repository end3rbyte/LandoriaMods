#!/usr/bin/env bash
set -euo pipefail

script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
if [[ "${1:-}" != --remote ]]; then
    # shellcheck disable=SC1091
    . "$script_directory/character-template.sh"
fi

usage() {
    echo "Usage: $0 deploy-test HOST MOD [MOD ...] | reconcile-test HOST | promote HOST MOD [MOD ...]" >&2
    exit 2
}

validate_host() {
    [[ "$1" =~ ^[A-Za-z0-9._-]+$ ]] || {
        echo "The storage executor hostname contains unsupported characters." >&2
        exit 2
    }
}

validate_relative_path() {
    [[ "$1" =~ ^server/(common|hammer|normal)/mods/(plugins/[A-Za-z0-9._-]+\.dll|config/([A-Za-z0-9._-]+/)?[A-Za-z0-9._-]+\.dll|config/CharacterTemplate\.yml)$ ]]
}

stage_character_templates() {
    local configuration="$1" environment="$2" variant items output
    mkdir -p "$staging_directory/configs"
    for variant in common hammer normal; do
        items="$(configured_items "$configuration" "$variant")" || continue
        output="$staging_directory/configs/$variant-CharacterTemplate.yml"
        render_character_template "$items" "$output"
        printf 'upload-config\t%s\t%s/server/%s/mods/config/CharacterTemplate.yml\n' \
            "${output##*/}" "$environment" "$variant" >> "$operations_file"
    done
}

mod_paths() {
    local configuration="$1" plugin="$2"
    jq -r --arg plugin "$plugin" '
        [paths(scalars) as $path |
            select(getpath($path) == $plugin) |
            ($path[0:-1] + [($plugin + ".dll")]) | join("/")] |
        unique[]
    ' <<< "$configuration"
}

normalize() {
    tr '[:upper:]' '[:lower:]' <<< "$1" | tr -cd '[:alnum:]'
}

modpack_dependency() {
    local plugin="$1" logical target dependency namespace package version
    logical="$plugin"
    [[ "$logical" == Landoria.* ]] && logical="${logical#Landoria.}"
    target="$(normalize "$logical")"
    while IFS= read -r dependency; do
        IFS=$'\t' read -r namespace package version < <(
            jq -rn --arg dependency "$dependency" '
                $dependency |
                capture("^(?<namespace>[^-]+)-(?<package>.+)-(?<version>[0-9]+\\.[0-9]+\\.[0-9]+)$") |
                [.namespace, .package, .version] | @tsv
            '
        )
        if [[ "$(normalize "$package")" == "$target" ]]; then
            printf '%s\t%s\t%s\n' "$namespace" "$package" "$version"
            return 0
        fi
    done < <(jq -r '.dependencies[]' \
        "$repository_root/Landoria.LandoriaModPack/manifest.json")
    return 1
}

stage_modpack_dll() {
    local plugin="$1" dependency namespace package version package_archive
    local archive_manifest entry entries target
    if [[ "$plugin" == Landoria.LandoriaModPack ]]; then
        dependency="$(jq -r '["Landoria", .name, .version_number] | @tsv' \
            "$repository_root/Landoria.LandoriaModPack/manifest.json")"
    else
        dependency="$(modpack_dependency "$plugin")" || return 1
    fi
    IFS=$'\t' read -r namespace package version <<< "$dependency"
    package_archive="$staging_directory/$namespace-$package-$version.zip"
    if [[ "$namespace" == Landoria ]]; then
        curl --fail --silent --show-error --location \
            "${LANDORIA_MOD_REPOSITORY_URL:-https://test.landoria-gaming.com:8443/api/v1/packages}/$namespace/$package/$version/download" \
            --output "$package_archive"
    else
        curl --fail --silent --show-error --location \
            "https://thunderstore.io/package/download/$namespace/$package/$version/" \
            --output "$package_archive"
    fi
    archive_manifest="$(unzip -p "$package_archive" manifest.json)"
    [[ "$(jq -er '.version_number' <<< "$archive_manifest")" == "$version" ]] || {
        echo "$namespace-$package archive manifest does not declare $version." >&2
        exit 1
    }
    target="$(normalize "$plugin")"
    entries="$({ unzip -Z1 "$package_archive" | while IFS= read -r entry; do
        [[ "$entry" == *.dll ]] || continue
        [[ "$(normalize "$(basename "$entry" .dll)")" == "$target" ]] && printf '%s\n' "$entry"
    done; } || true)"
    [[ -n "$entries" && "$(wc -l <<< "$entries")" -eq 1 ]] || {
        echo "Expected exactly one $plugin.dll in $namespace-$package-$version." >&2
        exit 1
    }
    entry="$entries"
    unzip -p "$package_archive" "$entry" > "$staging_directory/dlls/$plugin.dll"
}

add_dll_to_staging() {
    local mod="$1" plugin="$2" source
    source="$repository_root/Landoria.$mod/bin/Release/$plugin.dll"
    [[ -f "$source" ]] || {
        echo "Built DLL not found for $mod: $source" >&2
        exit 1
    }
    cp -- "$source" "$staging_directory/dlls/$plugin.dll"
}

plan_test_deployment() {
    local mod plugin destination found
    for mod in "$@"; do
        [[ "$mod" =~ ^[A-Za-z0-9]+$ ]] || usage
        plugin="Landoria.$mod"
        found=false
        while IFS= read -r destination; do
            [[ -n "$destination" ]] || continue
            validate_relative_path "$destination" || {
                echo "Invalid test DLL destination: $destination" >&2
                exit 1
            }
            if [[ "$found" == false ]]; then
                add_dll_to_staging "$mod" "$plugin"
                found=true
            fi
            printf 'upload\t%s\t%s\n' "$plugin.dll" "test/$destination" \
                >> "$operations_file"
        done < <(mod_paths "$LANDORIA_TEST_GAMEMODES_JSON" "$plugin")
        if [[ "$found" == false ]]; then
            echo "Skipping $plugin: it is not assigned to a test game mode."
        fi
    done
}

plan_test_reconciliation() {
    local plugin destination
    jq -r '
        paths(scalars) as $path |
        getpath($path) as $plugin |
        select($plugin | type == "string") |
        select($path | length >= 5) |
        select($path[-1] | type == "number") |
        "test/" + (($path[0:-1] + [($plugin + ".dll")]) | join("/"))
    ' <<< "$LANDORIA_TEST_GAMEMODES_JSON" | sort -u \
        > "$staging_directory/expected-paths.txt"
    while IFS= read -r destination; do
        printf 'test/server/%s/mods/config/CharacterTemplate.yml\n' "$destination"
    done < <(
        for destination in common hammer normal; do
            configured_items "$LANDORIA_TEST_GAMEMODES_JSON" "$destination" >/dev/null && \
                printf '%s\n' "$destination"
        done
    ) >> "$staging_directory/expected-paths.txt"
    sort -u -o "$staging_directory/expected-paths.txt" \
        "$staging_directory/expected-paths.txt"
    stage_character_templates "$LANDORIA_TEST_GAMEMODES_JSON" test
    while IFS= read -r plugin; do
        [[ -n "$plugin" ]] || continue
        if ! stage_modpack_dll "$plugin"; then
            continue
        fi
        while IFS= read -r destination; do
            [[ -n "$destination" ]] || continue
            validate_relative_path "$destination" || {
                echo "Invalid test DLL destination: $destination" >&2
                exit 1
            }
            printf 'upload\t%s\t%s\n' "$plugin.dll" "test/$destination" \
                >> "$operations_file"
        done < <(mod_paths "$LANDORIA_TEST_GAMEMODES_JSON" "$plugin")
    done < <(jq -r '
        paths(scalars) as $path |
        select($path[-1] | type == "number") |
        getpath($path) |
        select(type == "string")
    ' <<< "$LANDORIA_TEST_GAMEMODES_JSON" | sort -u)
}

plan_production_promotion() {
    local mod plugin source destination found
    for mod in "$@"; do
        [[ "$mod" =~ ^[A-Za-z0-9]+$ ]] || usage
        plugin="Landoria.$mod"
        source="$(mod_paths "$LANDORIA_TEST_GAMEMODES_JSON" "$plugin" | head -n 1)"
        found=false
        while IFS= read -r destination; do
            [[ -n "$destination" ]] || continue
            [[ -n "$source" ]] || {
                echo "$plugin has a production destination but no test source." >&2
                exit 1
            }
            if ! validate_relative_path "$source" || \
                ! validate_relative_path "$destination"; then
                echo "Invalid promotion path for $plugin." >&2
                exit 1
            fi
            found=true
            printf 'copy\t%s\t%s\n' "test/$source" "prod/$destination" \
                >> "$operations_file"
        done < <(mod_paths "$LANDORIA_PROD_GAMEMODES_JSON" "$plugin")
        if [[ "$found" == false ]]; then
            echo "Skipping $plugin: it is not assigned to a production game mode."
        fi
    done
    plan_production_character_templates
}

plan_production_character_templates() {
    local variant test_items production_items source destination
    for variant in common hammer normal; do
        source="test/server/$variant/mods/config/CharacterTemplate.yml"
        destination="prod/server/$variant/mods/config/CharacterTemplate.yml"
        production_items="$(configured_items "$LANDORIA_PROD_GAMEMODES_JSON" "$variant")" || {
            printf 'delete-if-exists\t-\t%s\n' "$destination" >> "$operations_file"
            continue
        }
        test_items="$(configured_items "$LANDORIA_TEST_GAMEMODES_JSON" "$variant")" || {
            echo "Production $variant items have no tested configuration." >&2
            exit 1
        }
        [[ "$test_items" == "$production_items" ]] || {
            echo "Test and production $variant items differ; promotion is unsafe." >&2
            exit 1
        }
        printf 'copy\t%s\t%s\n' "$source" "$destination" >> "$operations_file"
    done
}

refresh_manifests() (
    local environment="$1" website_base_url="$2" generator temporary_directory variant remote_path
    local cleanup_command
    generator="/opt/landoria-ops/valheim-podman/manual/generate-mod-manifest.py"
    [[ -x "$generator" ]] || {
        echo "The mod manifest generator is unavailable on the storage executor." >&2
        exit 1
    }
    temporary_directory="$(mktemp -d /tmp/landoria-mod-manifests.XXXXXXXX)"
    printf -v cleanup_command 'find %q -depth -delete' "$temporary_directory"
    # Capture the local path before the subshell EXIT trap runs.
    # shellcheck disable=SC2064
    trap "$cleanup_command" EXIT
    for variant in common hammer normal; do
        remote_path="storage:$SwissBackupStorage__Container/$environment/server/$variant/mods"
        mkdir -p "$temporary_directory/$variant/mods"
        rclone copy "$remote_path" "$temporary_directory/$variant/mods" \
            --exclude manifest.json
        "$generator" "$website_base_url/server/$variant/mods" \
            "$temporary_directory/$variant" --manifest-name "$variant-mods"
        rclone copyto "$temporary_directory/$variant/mods/manifest.json" \
            "$remote_path/manifest.json"
    done
)

run_remote_operations() {
    local archive="$1" credential_environment="$2" website_base_url="$3"
    local public_environment secret_environment temporary_directory
    local kind source destination destination_environment cleanup_command relative_path full_path
    [[ "$website_base_url" =~ ^https://[^[:space:]]+$ ]] || {
        echo "The website base URL must be an absolute HTTPS URL." >&2
        exit 2
    }
    public_environment="/etc/landoria-website-${credential_environment}.env"
    secret_environment="/var/lib/landoria-website-${credential_environment}-secrets/storage.env"
    [[ "$credential_environment" =~ ^[A-Za-z0-9_-]+$ ]]
    [[ -r "$public_environment" && -r "$secret_environment" ]] || {
        echo "The Swiss Backup environment is unavailable on the storage executor." >&2
        exit 1
    }
    SwissBackupStorage__User=''
    SwissBackupStorage__Password=''
    SwissBackupStorage__AuthUrl=''
    SwissBackupStorage__Project=''
    SwissBackupStorage__Domain=''
    SwissBackupStorage__Region=''
    SwissBackupStorage__Container=''
    set -a
    # shellcheck disable=SC1090
    . "$public_environment"
    # shellcheck disable=SC1090
    . "$secret_environment"
    set +a
    export RCLONE_CONFIG_STORAGE_TYPE=swift
    export RCLONE_CONFIG_STORAGE_USER="$SwissBackupStorage__User"
    export RCLONE_CONFIG_STORAGE_KEY="$SwissBackupStorage__Password"
    export RCLONE_CONFIG_STORAGE_AUTH="$SwissBackupStorage__AuthUrl"
    export RCLONE_CONFIG_STORAGE_TENANT="$SwissBackupStorage__Project"
    export RCLONE_CONFIG_STORAGE_DOMAIN="$SwissBackupStorage__Domain"
    export RCLONE_CONFIG_STORAGE_REGION="$SwissBackupStorage__Region"
    temporary_directory="$(mktemp -d /tmp/landoria-mod-storage.XXXXXXXX)"
    printf -v cleanup_command 'find %q -depth -delete' "$temporary_directory"
    # Capture the local path before the function EXIT trap runs.
    # shellcheck disable=SC2064
    trap "$cleanup_command" EXIT
    tar -xzf "$archive" -C "$temporary_directory"
    destination_environment=''
    while IFS=$'\t' read -r kind source destination; do
        [[ -n "$kind" ]] || continue
        [[ "$destination" =~ ^(test|prod)/ ]] || {
            echo "The remote DLL destination has no valid environment prefix." >&2
            exit 1
        }
        validate_relative_path "${destination#*/}" || {
            echo "Invalid remote DLL destination: $destination" >&2
            exit 1
        }
        case "$kind" in
            upload)
                [[ "$source" =~ ^[A-Za-z0-9._-]+\.dll$ && \
                   -f "$temporary_directory/dlls/$source" ]]
                rclone copyto "$temporary_directory/dlls/$source" \
                    "storage:$SwissBackupStorage__Container/$destination"
                ;;
            upload-config)
                [[ "$source" =~ ^(common|hammer|normal)-CharacterTemplate\.yml$ && \
                   -f "$temporary_directory/configs/$source" ]]
                rclone copyto "$temporary_directory/configs/$source" \
                    "storage:$SwissBackupStorage__Container/$destination"
                ;;
            copy)
                [[ "$source" =~ ^test/ ]] || {
                    echo "A promoted DLL must originate from test storage." >&2
                    exit 1
                }
                validate_relative_path "${source#*/}" || {
                    echo "Invalid remote DLL source: $source" >&2
                    exit 1
                }
                rclone copyto \
                    "storage:$SwissBackupStorage__Container/$source" \
                    "storage:$SwissBackupStorage__Container/$destination"
                ;;
            delete-if-exists)
                [[ "$source" == - && "$destination" == prod/*/CharacterTemplate.yml ]]
                rclone deletefile "storage:$SwissBackupStorage__Container/$destination" \
                    2>/dev/null || true
                echo "Removed obsolete $destination when present."
                continue
                ;;
            *)
                echo "Unsupported Swiss Backup operation: $kind" >&2
                exit 1
                ;;
        esac
        if [[ -z "$destination_environment" ]]; then
            destination_environment="${destination%%/*}"
        elif [[ "$destination_environment" != "${destination%%/*}" ]]; then
            echo "A single synchronization cannot target multiple environments." >&2
            exit 1
        fi
        rclone lsf "storage:$SwissBackupStorage__Container/$destination" \
            --files-only --max-depth 1 | grep -Fxq "${destination##*/}"
        echo "Synchronized $destination."
    done < "$temporary_directory/operations.tsv"
    if [[ -f "$temporary_directory/expected-paths.txt" ]]; then
        while IFS= read -r relative_path; do
            [[ -n "$relative_path" ]] || continue
            full_path="test/server/$relative_path"
            validate_relative_path "${full_path#test/}" || {
                echo "Invalid existing test DLL path: $full_path" >&2
                exit 1
            }
            if ! grep -Fxq "$full_path" "$temporary_directory/expected-paths.txt"; then
                rclone deletefile "storage:$SwissBackupStorage__Container/$full_path"
                echo "Deleted obsolete $full_path."
            fi
        done < <(rclone lsf "storage:$SwissBackupStorage__Container/test/server" \
            --recursive --files-only \
            --include '*.dll' --include 'CharacterTemplate.yml')
    fi
    [[ "$destination_environment" =~ ^(test|prod)$ ]]
    refresh_manifests "$destination_environment" "$website_base_url"
}

if [[ "${BASH_SOURCE[0]}" != "$0" ]]; then
    return 0
fi

if [[ "${1:-}" == --remote ]]; then
    [[ $# -eq 4 ]] || usage
    run_remote_operations "$2" "$3" "$4"
    exit 0
fi

[[ $# -ge 2 ]] || usage
[[ "${LANDORIA_STORAGE_BASE_URL:-}" =~ ^https://[^[:space:]]+$ ]] || {
    echo "LANDORIA_STORAGE_BASE_URL must be an absolute HTTPS URL." >&2
    exit 2
}
readonly operation="$1"
readonly target_host="$2"
shift 2
validate_host "$target_host"
repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
readonly repository_root
staging_directory="$(mktemp -d)"
readonly staging_directory
readonly operations_file="$staging_directory/operations.tsv"
readonly archive="$staging_directory/mod-storage.tar.gz"
readonly run_id="${GITHUB_RUN_ID:-local}"
readonly remote_script="/tmp/landoria-sync-mod-storage-${run_id}.sh"
readonly remote_archive="/tmp/landoria-sync-mod-storage-${run_id}.tar.gz"
trap 'rm -rf -- "$staging_directory"' EXIT
mkdir -p "$staging_directory/dlls"
: > "$operations_file"

case "$operation" in
    deploy-test)
        [[ $# -ge 1 ]] || usage
        [[ -n "${LANDORIA_TEST_GAMEMODES_JSON:-}" ]] || usage
        plan_test_deployment "$@"
        ;;
    reconcile-test)
        [[ $# -eq 0 && -n "${LANDORIA_TEST_GAMEMODES_JSON:-}" ]] || usage
        plan_test_reconciliation
        ;;
    promote)
        [[ $# -ge 1 ]] || usage
        [[ -n "${LANDORIA_TEST_GAMEMODES_JSON:-}" && \
           -n "${LANDORIA_PROD_GAMEMODES_JSON:-}" ]] || usage
        plan_production_promotion "$@"
        ;;
    *) usage ;;
esac

if [[ ! -s "$operations_file" ]]; then
    echo "No Swiss Backup DLL operation is required."
    exit 0
fi

archive_entries=(operations.tsv dlls)
[[ ! -d "$staging_directory/configs" ]] || archive_entries+=(configs)
[[ ! -f "$staging_directory/expected-paths.txt" ]] || archive_entries+=(expected-paths.txt)
tar -czf "$archive" -C "$staging_directory" "${archive_entries[@]}"
scp "$0" "$target_host:$remote_script"
scp "$archive" "$target_host:$remote_archive"
cleanup_remote() {
    ssh "$target_host" sudo find "$remote_script" "$remote_archive" \
        -maxdepth 0 -delete >/dev/null 2>&1 || true
}
trap 'cleanup_remote; rm -rf -- "$staging_directory"' EXIT
ssh "$target_host" sudo bash "$remote_script" --remote "$remote_archive" \
    test "$LANDORIA_STORAGE_BASE_URL"
