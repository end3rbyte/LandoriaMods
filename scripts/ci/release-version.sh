#!/usr/bin/env bash

next_patch_version() {
    local version="$1" major minor patch
    if [[ -z "$version" ]]; then
        printf '1.0.0\n'
        return
    fi
    [[ "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]] || {
        echo "Invalid released version: $version" >&2
        return 1
    }
    IFS=. read -r major minor patch <<< "$version"
    printf '%s.%s.%s\n' "$major" "$minor" "$((patch + 1))"
}

validate_next_release_version() {
    local package="$1" candidate="$2" latest_release="$3" expected
    [[ "$candidate" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]] || {
        echo "Invalid candidate version for $package: $candidate" >&2
        return 1
    }
    expected="$(next_patch_version "$latest_release")"
    [[ "$candidate" == "$expected" ]] || {
        echo "Landoria-$package test version $candidate is invalid: " \
            "Thunderstore currently has ${latest_release:-no release}, so the next version must be $expected." >&2
        return 1
    }
}
