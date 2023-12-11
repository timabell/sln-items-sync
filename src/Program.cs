using CommandLine;
using SlnEditor;
using SlnEditor.Contracts;
using SlnEditor.Helper;

namespace sln_items_sync;

public class Program
{
    private const string SolutionItemsName = "SolutionItems";
    private static SolutionParser _parser = new();
    private static Guid _solutionFolderTypeGuid = new ProjectTypeMapper().ToGuid(ProjectType.SolutionFolder);

    public class Options
    {
        [Option('s', "solution", Required = true, HelpText = "path to .sln file to modify")]
        public string SlnPath { get; set; }

        [CommandLine.Value(0)]
        public IEnumerable<string> Paths { get; set; }
    }

    public static int Main(string[] args)
    {
        var parserResult = Parser.Default.ParseArguments<Options>(args);
        if (parserResult.Errors.Any())
        {
            return 1;
        }

        parserResult
            .WithParsed(opts => SyncPaths(slnPath: opts.SlnPath, opts.Paths));
        return 0;
    }

    /// <summary>
    /// Update "SolutionItems" folder in sln based on filesystem files / folders passed in.
    /// - files will be added if missing
    /// - folders will be forced to recursively match the filesystem
    /// </summary>
    /// <param name="slnPath">relative path to sln file to modify</param>
    /// <param name="paths">list of paths to recursively add/update SolutionItems virtual folders with</param>
    public static void SyncPaths(string slnPath, IEnumerable<string> paths)
    {
        var solution = _parser.Parse(slnPath);

        var solutionItems = FindOrCreateSolutionFolder(solution.Projects, SolutionItemsName, SolutionItemsName);

        foreach (var path in paths)
        {
            if (File.Exists(path) && solutionItems.Files.All(f => f.Name != path))
            {
                solutionItems.Files.Add(new FileInfo(path));
            }
            else if (Directory.Exists(path))
            {
                SyncFolder(solutionItems, new DirectoryInfo(path));
            }
            else
            {
                throw new Exception($"path not found: '{path}'");
            }
        }


        var updatedSln = solution.Write();
        File.WriteAllText(slnPath, updatedSln);
    }

    private static void SyncFolder(SolutionFolder parentFolder, DirectoryInfo directory)
    {
        var solutionFolder = FindOrCreateSolutionFolder(parentFolder.Projects, directory.Name, $"{directory.Name}/");
        foreach (var file in directory.EnumerateFiles())
        {
            if (solutionFolder.Files.All(f => f.Name != file.Name))
            {
                solutionFolder.Files.Add(file);
            }
        }
        foreach (var subDirectory in directory.EnumerateDirectories())
        {
            SyncFolder(solutionFolder, subDirectory);
        }

        // todo
        // var solutionFolder = new SolutionFolder(Guid.NewGuid(), "foo", "foo/", _solutionFolderTypeGuid, ProjectType.SolutionFolder);
        //
        // var nestedSolutionFolder = new SolutionFolder(Guid.NewGuid(), "bar", "bar/", _solutionFolderTypeGuid, ProjectType.SolutionFolder);
        // solutionFolder.Projects.Add(nestedSolutionFolder);
        // solution.Projects.Add(solutionFolder);
    }

    private static SolutionFolder FindOrCreateSolutionFolder(ICollection<IProject> solutionProjects,
        string solutionFolderName, string path)
    {
        var solutionItems = FindSolutionFolder(solutionProjects, solutionFolderName);
        if (solutionItems is not null)
        {
            return solutionItems;
        }

        solutionItems = new SolutionFolder(id: Guid.NewGuid(), name: solutionFolderName, path: path,
            typeGuid: _solutionFolderTypeGuid, ProjectType.SolutionFolder);
        solutionProjects.Add(solutionItems);

        return solutionItems;
    }

    private static SolutionFolder? FindSolutionFolder(IEnumerable<IProject> solutionProjects, string folderName)
        => solutionProjects.OfType<SolutionFolder>().FirstOrDefault(project => project.Name == folderName);
}
