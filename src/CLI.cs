using CommandLine;
using SlnEditor;
using SlnEditor.Contracts;
using SlnEditor.Helper;
using SystemInterface.IO;

namespace sln_items_sync;

public class CLI(IFile file, IDirectory directory, IGuidGenerator? guidGenerator = null)
{
	private readonly IGuidGenerator _guidGenerator = guidGenerator ?? new DefaultGuidGenerator();
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

	public int Run(string[] args)
	{
		var parserResult = Parser.Default.ParseArguments<Options>(args);
		if (parserResult.Errors.Any())
		{
			return 1;
		}

		parserResult
			.WithParsed(opts => SyncSlnFile(slnPath: opts.SlnPath, opts.Paths));
		return 0;
	}

	/// <summary>
	/// Update "SolutionItems" folder in sln based on filesystem files / folders passed in.
	/// - files will be added if missing
	/// - folders will be forced to recursively match the filesystem
	/// </summary>
	/// <param name="slnPath">relative path to sln file to modify</param>
	/// <param name="paths">list of paths to recursively add/update SolutionItems virtual folders with</param>
	public void SyncSlnFile(string slnPath, IEnumerable<string> paths)
	{
		var contents = file.ReadAllText(slnPath);
		var updatedSln = SyncSlnText(contents, paths);
		file.WriteAllText(slnPath, updatedSln);
	}

	// todo: move to sensible files
	public string SyncSlnText(string contents, IEnumerable<string> paths)
	{
		var solution = _parser.ParseText(contents);

		var solutionItems = FindOrCreateSolutionFolder(solution.Projects, SolutionItemsName, SolutionItemsName);

		foreach (var path in paths)
		{
			if (file.Exists(path))
			{
				SyncFile(solutionItems, path);
			}
			else if (directory.Exists(path))
			{
				SyncFolder(solutionItems, dir: path, path: $"{path}\\");
			}
			else
			{
				throw new Exception($"path not found: '{path}'");
			}
		}


		var updatedSln = solution.Write();
		return updatedSln;
	}

	private static void SyncFile(SolutionFolder solutionItems, string path)
	{
		if (solutionItems.Files.All(f => f != path))
		{
			solutionItems.Files.Add(path);
		}
	}

	private void SyncFolder(SolutionFolder parentFolder, string dir, string path)
	{
		var solutionFolder = FindOrCreateSolutionFolder(parentFolder.Projects, dir, dir);
		foreach (var file in directory.EnumerateFiles(dir))
		{
			if (solutionFolder.Files.All(f => f != file))
			{
				solutionFolder.Files.Add($"{path}{file}");
			}
		}
		foreach (var subDirectory in directory.GetDirectories(dir))
		{
			SyncFolder(solutionFolder, subDirectory, $"{path}{subDirectory}\\");
		}

		// todo
		// var solutionFolder = new SolutionFolder(Guid.NewGuid(), "foo", "foo/", _solutionFolderTypeGuid, ProjectType.SolutionFolder);
		//
		// var nestedSolutionFolder = new SolutionFolder(Guid.NewGuid(), "bar", "bar/", _solutionFolderTypeGuid, ProjectType.SolutionFolder);
		// solutionFolder.Projects.Add(nestedSolutionFolder);
		// solution.Projects.Add(solutionFolder);
	}

	private SolutionFolder FindOrCreateSolutionFolder(ICollection<IProject> solutionProjects,
		string solutionFolderName, string path)
	{
		var solutionItems = FindSolutionFolder(solutionProjects, solutionFolderName);
		if (solutionItems is not null)
		{
			return solutionItems;
		}

		solutionItems = new SolutionFolder(id: _guidGenerator.Next(), name: solutionFolderName, path: path,
			typeGuid: _solutionFolderTypeGuid, ProjectType.SolutionFolder);
		solutionProjects.Add(solutionItems);

		return solutionItems;
	}

	private static SolutionFolder? FindSolutionFolder(IEnumerable<IProject> solutionProjects, string folderName)
		=> solutionProjects.OfType<SolutionFolder>().FirstOrDefault(project => project.Name == folderName);
}

public class DefaultGuidGenerator : IGuidGenerator
{
	public Guid Next()
	{
		return Guid.NewGuid();
	}
}

public interface IGuidGenerator
{
	Guid Next();
}
