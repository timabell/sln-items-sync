#!/bin/sh -v
set -e # exit on error

latest=`mise ls-remote dotnet-core | grep -P '^\d+\.\d+\.\d+$' | sort --version-sort | tail -n 1`
echo "Updating to dotnet-core@$latest"

mise use "dotnet-core@$latest"

# Extract major version (e.g., "10" from "10.0.100")
major_version=$(echo "$latest" | cut -d. -f1)
target_framework="net${major_version}.0"
echo "Updating TargetFramework to $target_framework"

# Update all csproj files
find . -name "*.csproj" -exec sed -i "s|<TargetFramework>net[0-9]*\.0</TargetFramework>|<TargetFramework>${target_framework}</TargetFramework>|g" {} \;

dotnet test

git add .tool-versions **/*.csproj
git commit --message "chore: Upgrade build to dotnet $latest"
