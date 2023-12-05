using SlnEditor;
using SlnEditor.Contracts;
using SlnEditor.Helper;

namespace sln_items_sync;

public class Program
{
    public static int Main(string[] args)
    {
        var parser = new SolutionParser();
        var solutionFilePath = args[0];
        var solution = parser.Parse(solutionFilePath);

        var solutionFolderTypeGuid = new ProjectTypeMapper().ToGuid(ProjectType.SolutionFolder);
        var solutionFolder = new SolutionFolder(Guid.NewGuid(), "foo", "foo/", solutionFolderTypeGuid, ProjectType.SolutionFolder);
        var nestedSolutionFolder = new SolutionFolder(Guid.NewGuid(), "bar", "bar/", solutionFolderTypeGuid, ProjectType.SolutionFolder);
        solutionFolder.Projects.Add(nestedSolutionFolder);
        solution.Projects.Add(solutionFolder);
        var updatedSln = solution.Write();
        parser.Write(solution, solutionFilePath);

        Console.WriteLine("fin");
        return 0;
    }
}