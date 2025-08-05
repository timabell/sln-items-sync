#!/bin/sh -v
set -e # exit on error
asdf plugin update dotnet-core
# filter out -preview releases, and assume last one is newest
latest=`asdf list all dotnet-core | grep -v "-" | tail -n 2`
echo $latest
asdf install dotnet-core $latest
asdf set dotnet-core $latest
dotnet test
git commit --include .tool-versions --message "Upgrade dotnet-core to latest"
