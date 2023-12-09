#!/bin/sh -v
set -e
cd tests/data/
dotnet run -p ../../src/sln-items-sync.csproj --solution example.sln contrib.txt README.md .github
