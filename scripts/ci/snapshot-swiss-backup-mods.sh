#!/usr/bin/env bash
set -euo pipefail

usage() {
    echo "Usage: $0 ENVIRONMENT HOST OUTPUT_DIRECTORY" >&2
    exit 2
}

load_storage_environment() {
    local environment="$1"
    local public_environment="/etc/landoria-website-${environment}.env"
    local secret_environment="/var/lib/landoria-website-${environment}-secrets/storage.env"
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
}

create_snapshot() {
    local environment="$1" archive="$2" temporary_directory cleanup_command
    load_storage_environment "$environment"
    temporary_directory="$(mktemp -d /tmp/landoria-mod-snapshot.XXXXXXXX)"
    printf -v cleanup_command 'find %q -depth -delete' "$temporary_directory"
    # Capture the path before the EXIT trap runs.
    # shellcheck disable=SC2064
    trap "$cleanup_command" EXIT
    mkdir -p "$temporary_directory/server"
    rclone copy \
        "storage:$SwissBackupStorage__Container/$environment/server" \
        "$temporary_directory/server"
    tar -czf "$archive" -C "$temporary_directory" server
}

if [[ "${1:-}" == --remote ]]; then
    [[ $# -eq 3 && "$2" =~ ^(test|prod)$ ]] || usage
    create_snapshot "$2" "$3"
    exit 0
fi

[[ $# -eq 3 && "$1" =~ ^(test|prod)$ ]] || usage
environment="$1"
target_host="$2"
output_directory="$3"
[[ "$target_host" =~ ^[A-Za-z0-9._-]+$ ]] || usage
run_id="${GITHUB_RUN_ID:-local}"
remote_script="/tmp/landoria-snapshot-mod-storage-${run_id}.sh"
remote_archive="/tmp/landoria-snapshot-mod-storage-${run_id}.tar.gz"
local_archive="$(mktemp)"
cleanup() {
    ssh "$target_host" sudo find "$remote_script" "$remote_archive" \
        -maxdepth 0 -delete >/dev/null 2>&1 || true
    find "$local_archive" -maxdepth 0 -delete >/dev/null 2>&1 || true
}
trap cleanup EXIT
mkdir -p "$output_directory"
scp "$0" "$target_host:$remote_script"
ssh "$target_host" sudo bash "$remote_script" --remote \
    "$environment" "$remote_archive"
scp "$target_host:$remote_archive" "$local_archive"
tar -xzf "$local_archive" -C "$output_directory"
