#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
# shellcheck source-path=SCRIPTDIR
# shellcheck source=changelog.sh
source "$repository_root/scripts/ci/changelog.sh"

test_root="$(mktemp -d)"
trap 'rm -rf -- "$test_root"' EXIT
mkdir -p "$test_root/Landoria.Example"
git -C "$test_root" init -q
git -C "$test_root" config user.name changelog-test
git -C "$test_root" config user.email changelog-test@users.noreply.github.com
cat > "$test_root/Landoria.Example/CHANGELOG.md" <<'EOF'
# Changelog

## 1.0.0

- Initial release.
EOF
git -C "$test_root" add .
git -C "$test_root" commit -qm 'Initial release'
git -C "$test_root" tag thunderstore/Example/1.0.0

initial_hash="$(sha256sum "$test_root/Landoria.Example/CHANGELOG.md")"
update_unreleased_changelog "$test_root" Example 1.0.1
[[ "$(sha256sum "$test_root/Landoria.Example/CHANGELOG.md")" == "$initial_hash" ]]
if grep -Fqx '## Unreleased' "$test_root/Landoria.Example/CHANGELOG.md"; then
    echo 'An empty Unreleased section was generated.' >&2
    exit 1
fi

printf 'feature\n' > "$test_root/Landoria.Example/feature.txt"
git -C "$test_root" add .
git -C "$test_root" commit -qm 'Add example feature'
update_unreleased_changelog "$test_root" Example 1.0.1
grep -Fqx '## Unreleased' "$test_root/Landoria.Example/CHANGELOG.md"
grep -Fqx -- '- Add example feature' "$test_root/Landoria.Example/CHANGELOG.md"

initial_hash="$(sha256sum "$test_root/Landoria.Example/CHANGELOG.md")"
update_unreleased_changelog "$test_root" Example 1.0.1
[[ "$(sha256sum "$test_root/Landoria.Example/CHANGELOG.md")" == "$initial_hash" ]]

git -C "$test_root" add .
git -C "$test_root" commit -qm 'Release updated public mods'
printf 'second feature\n' > "$test_root/Landoria.Example/second-feature.txt"
git -C "$test_root" add .
git -C "$test_root" commit -qm 'Add another feature'
update_unreleased_changelog "$test_root" Example 1.0.1
grep -Fqx -- '- Add another feature' "$test_root/Landoria.Example/CHANGELOG.md"
grep -Fqx -- '- Add example feature' "$test_root/Landoria.Example/CHANGELOG.md"
if grep -Fq -- '- Release updated public mods' "$test_root/Landoria.Example/CHANGELOG.md"; then
    echo 'Generated changelog contains a release automation commit.' >&2
    exit 1
fi

sed -i 's/^## Unreleased$/## 1.0.1/' "$test_root/Landoria.Example/CHANGELOG.md"
release_hash="$(sha256sum "$test_root/Landoria.Example/CHANGELOG.md")"
update_unreleased_changelog "$test_root" Example 1.0.1
[[ "$(sha256sum "$test_root/Landoria.Example/CHANGELOG.md")" == "$release_hash" ]]
validate_release_changelog "$test_root/Landoria.Example/CHANGELOG.md" 1.0.1

sed -i 's/^## 1.0.1$/## Unreleased/' "$test_root/Landoria.Example/CHANGELOG.md"
if validate_release_changelog "$test_root/Landoria.Example/CHANGELOG.md" 1.0.1 2>/dev/null; then
    echo 'Validation unexpectedly accepted Unreleased.' >&2
    exit 1
fi
sed -i 's/^## Unreleased$/## 1.0.1/' "$test_root/Landoria.Example/CHANGELOG.md"
if validate_release_changelog "$test_root/Landoria.Example/CHANGELOG.md" 1.0.2 2>/dev/null; then
    echo 'Validation unexpectedly accepted a missing release section.' >&2
    exit 1
fi

echo 'Changelog tests passed.'
