#!/usr/bin/env bash

changelog_has_section() {
    local changelog="$1" section="$2"
    awk -v expected="$section" '
        /^##[[:space:]]+/ {
            heading = $0
            sub(/^##[[:space:]]+/, "", heading)
            sub(/[[:space:]]+-.*$/, "", heading)
            sub(/[[:space:]]+$/, "", heading)
            if (heading == expected) found = 1
        }
        END { exit !found }
    ' "$changelog"
}

remove_unreleased_section() {
    local changelog="$1"
    awk '
        /^##[[:space:]]+Unreleased([[:space:]]+-.*)?[[:space:]]*$/ {
            skipping = 1
            next
        }
        skipping && /^##[[:space:]]+/ {
            skipping = 0
        }
        !skipping { print }
    ' "$changelog"
}

unreleased_entries() {
    local repository_root="$1" mod="$2" tag range
    tag="$(git -C "$repository_root" tag --list "thunderstore/$mod/*" \
        --sort=-v:refname | head -n 1)"
    range="HEAD"
    [[ -z "$tag" ]] || range="$tag..HEAD"
    git -C "$repository_root" log --format='%s' --no-merges --invert-grep \
        --grep='^Release updated public mods$' \
        --grep='^Prepare Thunderstore releases$' \
        "$range" -- "Landoria.$mod"
}

update_unreleased_changelog() {
    local repository_root="$1" mod="$2" version="$3" changelog temporary subject
    local -a subjects=()
    changelog="$repository_root/Landoria.$mod/CHANGELOG.md"
    changelog_has_section "$changelog" "$version" && return 0

    mapfile -t subjects < <(unreleased_entries "$repository_root" "$mod")
    temporary="$changelog.tmp"
    {
        printf '# Changelog\n\n## Unreleased\n'
        if [[ ${#subjects[@]} -gt 0 ]]; then
            printf '\n'
            for subject in "${subjects[@]}"; do
                [[ -z "$subject" ]] || printf -- '- %s\n' "$subject"
            done
        fi
        printf '\n'
        remove_unreleased_section "$changelog" | tail -n +3 | sed '/./,$!d'
    } > "$temporary"
    mv -- "$temporary" "$changelog"
}

validate_release_changelog() {
    local changelog="$1" version="$2"
    if changelog_has_section "$changelog" Unreleased; then
        echo "The changelog still contains an Unreleased section: $changelog" >&2
        return 1
    fi
    if ! changelog_has_section "$changelog" "$version"; then
        echo "The changelog has no section for release $version: $changelog" >&2
        return 1
    fi
}
