#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
promotion_script="$repository_root/scripts/ci/promote-mods.sh"

grep -Fq 'download_internal_draft "$package" "$current" "$archive"' "$promotion_script"
grep -Fq 'actual_hash="$(sha256sum "$target"' "$promotion_script"
grep -Fq 'validate_release_changelog "$archived_changelog" "$version"' "$promotion_script"
grep -Fq 'is already released; no package rebuild is required' "$promotion_script"
grep -Fq 'validate_promotable_package "$package"' "$promotion_script"
grep -Fq 'not included in GLOBAL_VARS.yml modpack.landoria_packages' "$promotion_script"

if grep -Fq '"$repository_root/scripts/ci/publish-mods.sh"' "$promotion_script" || \
   grep -Eq 'dotnet build|replace_version' "$promotion_script"; then
    echo 'Promotion must not rebuild or mutate a tested package.' >&2
    exit 1
fi

echo 'Promotion immutability tests passed.'
