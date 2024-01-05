# SolutionItems sync tool

Run this against a `.sln` file with a list of paths and it will update the solution items to match

## Usage

Files will be added if missing.

Folders will be recursively added.

`SolutionItems` folder will be created and populated if missing.

### Local installation

```bash
# if you don't have a manifest file yet
dotnet new tool-manifest

dotnet tool install --local sln-items-sync

# run the tool
dotnet sln-items-sync --solution YourSolution.sln README.md .github
```

### Global installation

```bash
dotnet tool install --global sln-items-sync

# run the tool
sln-items-sync --solution YourSolution.sln README.md .github
```
