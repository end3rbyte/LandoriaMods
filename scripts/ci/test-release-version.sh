#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
# shellcheck source-path=SCRIPTDIR
# shellcheck source=release-version.sh
source "$repository_root/scripts/ci/release-version.sh"

[[ "$(next_patch_version '')" == 1.0.0 ]]
[[ "$(next_patch_version 1.0.0)" == 1.0.1 ]]
[[ "$(next_patch_version 2.7.19)" == 2.7.20 ]]

validate_next_release_version Example 1.0.0 ''
validate_next_release_version Example 1.0.1 1.0.0
validate_next_release_version Example 2.7.20 2.7.19

if validate_next_release_version Example 1.0.2 1.0.0 2>/dev/null; then
    echo 'Validation unexpectedly accepted a skipped patch version.' >&2
    exit 1
fi
if validate_next_release_version Example 1.0.0 1.0.0 2>/dev/null; then
    echo 'Validation unexpectedly accepted the current production version.' >&2
    exit 1
fi
if validate_next_release_version Example 1.0.1 '' 2>/dev/null; then
    echo 'Validation unexpectedly accepted an invalid initial version.' >&2
    exit 1
fi

echo 'Release version tests passed.'
