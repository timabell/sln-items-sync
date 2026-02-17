#!/bin/sh -v
set -e # exit on error

latest=`mise ls-remote dotnet-core | grep -P '^\d+\.\d+\.\d+$' | sort --version-sort | tail -n 1`
echo "Updating to dotnet-core@$latest"

mise use "dotnet-core@$latest"

dotnet test

git commit -i .tool-versions -m "chore: Upgrade build to latest dotnet"
