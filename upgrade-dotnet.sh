#!/bin/sh -v
set -e # exit on error

latest=`mise ls-remote dotnet-core | grep -P '^\d+\.\d+\.\d+$' | sort --version-sort | tail -n 1`
echo "Updating to dotnet-core@$latest"

mise use "dotnet-core@$latest"

# Extract major version (e.g., "10" from "10.0.100")
major_version=$(echo "$latest" | cut -d. -f1)
target_framework="net${major_version}.0"
echo "Updating TargetFramework to $target_framework"

# Update test project (single TFM - always tracks latest)
sed -i "s|<TargetFramework>net[0-9]*\.0</TargetFramework>|<TargetFramework>${target_framework}</TargetFramework>|g" tests/tests.csproj

# Update src project (multi-TFM - append new version to keep backwards compatibility)
if ! grep -q "${target_framework}" src/sln-items-sync.csproj; then
  sed -i "s|</TargetFrameworks>|;${target_framework}</TargetFrameworks>|g" src/sln-items-sync.csproj
fi

dotnet test

git add .tool-versions **/*.csproj
git commit --message "chore: Upgrade build to dotnet $latest"
