#!/bin/sh -v
set -e # exit on error
asdf plugin update dotnet-core
latest=`asdf list all dotnet-core | tail -n 1`
echo $latest
asdf install dotnet-core $latest
asdf local dotnet-core $latest
dotnet test
git commit --include .tool-versions --message "Upgrade dotnet-core to latest"
