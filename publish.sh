#!/bin/sh -v
set -e
git clean -xfd
dotnet pack
dotnet nuget push src/bin/Release/*.nupkg --source https://api.nuget.org/v3/index.json --api-key="$apikey"
