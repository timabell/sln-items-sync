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

`Solution Items` folder will be used, or created if missing (matching Visual Studio's default). Existing `SolutionItems` folder (no space) is also detected. The name can be customized with `--folder`.

If there is only one `.sln` file in the current folder then the `--solution` argument can be omitted and it will automatically be found.

## License

- [A-GPL v3](LICENSE)

## Development

This tool is made possible by the [SlnEditor nuget package](https://github.com/timabell/SlnEditor) which provides parsing of `.sln` files, the ability to modify them in-memory with an object model, and then to write them back to string format with ordering largely preserved intact.

## Build & Release

Automated CI/CD using GitHub Actions. For full details on this setup see [github-nuget-demo](https://github.com/timabell/github-nuget-demo).

### Pipeline

- **Pull requests**: Build and test runs automatically
- **Main branch commits**: If release-worthy changes exist, automatically:
  1. Calculates next version using [git-cliff](https://git-cliff.org/)
  2. Generates changelog from conventional commits
  3. Creates git tag and GitHub Release
  4. Publishes to [nuget.org](https://www.nuget.org/packages/sln-items-sync/) and GitHub Packages

### Versioning

Semantic versioning with automatic patch increments. To bump major or minor version, add a footer to your commit message:

```
feat: add new export format

bump: minor
```

### Conventional Commits

Use semantic prefixes for changes that should appear in release notes:

| Prefix | Purpose |
|--------|---------|
| `feat:` | New features |
| `fix:` | Bug fixes |
| `doc:` | Documentation |
| `perf:` | Performance improvements |

Internal prefixes (excluded from releases): `refactor:`, `test:`, `ci:`, `build:`, `chore:`

Commits without these prefixes won't trigger a release.

There is a suite of tests that use a real filesystem to provide end to end coverage of the tool, only stopping short of testing the real binary (instead calling the same entry point as the main program). Any pull requests need to show their modified behaviour in these tests.

There are .sh scripts for upgrading dotnet-core and nuget dependencies in the repo root. With the tests we can ship these patches with high confidence.

The dotnet version last worked with locally is controlled by [.tool-versions](.tool-versions) which allows [mise](https://mise.jdx.dev/) to make the correct version available. If you don't already have it, install it and then run `mise install` to set up dotnet-core at the right version. It's a great tool so well worth taking the time.

## Backstory

If you're interested in how this came about read [the genesis story of sln-items-sync](https://0x5.uk/2024/01/13/new-tool-sln-items-sync-for-visual-studio-solution-folders/) on my blog.
