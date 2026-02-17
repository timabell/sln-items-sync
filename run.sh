#!/bin/sh -v
dotnet run --project src/sln-items-sync.csproj --framework net10.0 --solution sln-items-sync.sln .github *.sh LICENSE
