# SolutionItems sync tool

Run this against a `.sln` file with a list of paths and it will update the solution items to match

Usage

```
dotnet run --project src/sln-items-sync.csproj --solution sln-items-sync.sln README.md .github
```

Files will be added if missing.

Folders will be recursively added.

`SolutionItems` folder will be created and populated if missing.
