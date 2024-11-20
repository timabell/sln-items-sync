# SolutionItems sync tool

Run this against a `.sln` file with a list of paths and it will update the solution items to match

[![Latest Release](https://img.shields.io/nuget/v/sln-items-sync)](https://www.nuget.org/packages/sln-items-sync/)
[![Downloads](https://img.shields.io/nuget/dt/sln-items-sync)](https://www.nuget.org/packages/sln-items-sync/)

## Install

```sh
dotnet tool install --global sln-items-sync
```

You'll need `~/.dotnet/tools` on the PATH if it isn't already.

## Update

```sh
dotnet tool update -g sln-items-sync
```

## Usage

```sh
sln-items-sync --solution sln-items-sync.sln README.md .github
```

Files and folders will be recursively add/removed to match the filesystem.

`SolutionItems` folder will be created and populated if missing. The name can be customized with `--folder`.

If there is only one `.sln` file in the current folder then the `--solution` argument can be omitted and it will automatically be found.

## License

- [A-GPL v3](LICENSE)

## Development

This tool is made possible by the [SlnEditor nuget package](https://github.com/timabell/SlnEditor) which provides parsing of `.sln` files, the ability to modify them in-memory with an object model, and then to write them back to string format with ordering largely preserved intact.

There is a suite of tests that use a real filesystem to provide end to end coverage of the tool, only stopping short of testing the real binary (instead calling the same entry point as the main program). Any pull requests need to show their modified behaviour in these tests.

There are .sh scripts for upgrading dotnet-core and nuget dependencies in the repo root. With the tests we can ship these patches with high confidence.

The dotnet version last worked with locally is controlled by [.tool-versions](.tool-versions) which allows [asdf-vm](https://asdf-vm.com/) to make the correct version available. If you don't already have it, install it and then run `asdf install` to set up dotnet-core at the right version. It's a great tool so well worth taking the time.

## Backstory

If you're interested in how this came about read [the genesis story of sln-items-sync](https://timwise.co.uk/2024/01/13/new-tool-sln-items-sync-for-visual-studio-solution-folders/) on my blog.
