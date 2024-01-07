# SolutionItems sync tool

Run this against a `.sln` file with a list of paths and it will update the solution items to match

Install

```sh
dotnet tool install --global sln-items-sync
```

You'll need `~/.dotnet/tools` on the PATH if it isn't already.

Usage

```sh
sln-items-sync --solution sln-items-sync.sln README.md .github
```

Files will be added if missing.

Folders will be recursively added.

`SolutionItems` folder will be created and populated if missing.
