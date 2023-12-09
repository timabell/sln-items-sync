using CommandLine;
using SlnEditor;
using SlnEditor.Contracts;
using SlnEditor.Helper;

namespace sln_items_sync;

public class Program
{
    public class Options
    {
        [Option('s', "solution", Required = true, HelpText = "path to .sln file to modify")]
        public string SlnPath { get; set; }

        [Option]
        public IList<string> Paths { get; set; }
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
    public static void SyncPaths(string slnPath, IList<string> paths)
    {
        var parser = new SolutionParser();
        var solution = parser.Parse(slnPath);
        var solutionFolderTypeGuid = new ProjectTypeMapper().ToGuid(ProjectType.SolutionFolder);
        var solutionFolder = new SolutionFolder(Guid.NewGuid(), "foo", "foo/", solutionFolderTypeGuid, ProjectType.SolutionFolder);
        var nestedSolutionFolder = new SolutionFolder(Guid.NewGuid(), "bar", "bar/", solutionFolderTypeGuid, ProjectType.SolutionFolder);
        solutionFolder.Projects.Add(nestedSolutionFolder);
        solution.Projects.Add(solutionFolder);
        var updatedSln = solution.Write();
        File.WriteAllText(slnPath, updatedSln);
    }
}