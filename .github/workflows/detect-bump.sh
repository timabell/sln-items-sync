#!/bin/sh
# Detect version bump type from commit footers since last tag
# Outputs: major, minor, or empty (for patch)
#
# Why not use git-cliff's custom_minor_increment_regex?
# git-cliff's regex only matches against the conventional commit TYPE
# (the part before ':', '!', or '('), not the full commit message or body.
# This means footers like "bump: minor" can't be matched, and we'd have to
# use commit types like "minor:" which conflicts with semantic types like
# "feat:" or "fix:" that we want in the changelog.

LAST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "")

if [ -z "$LAST_TAG" ]; then
  COMMITS=$(git log --format=%B)
else
  COMMITS=$(git log "$LAST_TAG"..HEAD --format=%B)
fi

if echo "$COMMITS" | grep -qE '^bump: major$'; then
  echo "major"
elif echo "$COMMITS" | grep -qE '^bump: minor$'; then
  echo "minor"
fi
