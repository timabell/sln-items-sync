using CommandLine;
using SlnEditor;
using SlnEditor.Contracts;
using SlnEditor.Helper;

namespace sln_items_sync;

public class CLI(IGuidGenerator? guidGenerator = null)
{
	private readonly IGuidGenerator _guidGenerator = guidGenerator ?? new DefaultGuidGenerator();
	private static SolutionParser _parser = new();
	private static Guid _solutionFolderTypeGuid = new ProjectTypeMapper().ToGuid(ProjectType.SolutionFolder);

	public class Options
	{
		[Option('s', "solution", Required = true, HelpText = "path to .sln file to modify")]
		public string SlnPath { get; set; }

		[Option('f', "folder", Required = false, HelpText = "Solution folder to target")]
		public string SlnFolder { get; set; } = "SolutionItems";

		[Value(0)]
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
			.WithParsed(opts => SyncSlnFile(slnPath: opts.SlnPath, slnFolder: opts.SlnFolder, opts.Paths));
		return 0;
	}

	/// <summary>
	/// Update "SolutionItems" folder in sln based on filesystem files / folders passed in.
	/// - files will be added if missing
	/// - folders will be forced to recursively match the filesystem
	/// </summary>
	/// <param name="slnPath">relative path to sln file to modify</param>
	/// <param name="slnFolder"></param>
	/// <param name="paths">list of paths to recursively add/update SolutionItems virtual folders with</param>
	public void SyncSlnFile(string slnPath, string slnFolder, IEnumerable<string> paths)
	{
		var contents = File.ReadAllText(slnPath);
		var updatedSln = SyncSlnText(contents, slnFolder, paths);
		File.WriteAllText(slnPath, updatedSln);
	}

	// todo: allow mocking filesystem calls
	// todo: move to sensible files
	public string SyncSlnText(string contents, string slnFolder, IEnumerable<string> paths)
	{
		var solution = _parser.ParseText(contents);

		var solutionItems = FindOrCreateSolutionFolder(solution.Projects, slnFolder, slnFolder);

		foreach (var path in paths)
		{
			if (File.Exists(path))
			{
				SyncFile(solutionItems, path);
			}
			else if (Directory.Exists(path))
			{
				SyncFolder(solutionItems, new DirectoryInfo(path), $"{path}\\");
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

	private void SyncFolder(SolutionFolder parentFolder, DirectoryInfo directory, string path)
	{
		var solutionFolder = FindOrCreateSolutionFolder(parentFolder.Projects, directory.Name, directory.Name);
		var files = directory.GetFiles();
		foreach (var file in files)
		{
			if (solutionFolder.Files.Select(f => f.SlnItemName()).All(f => f != file.Name))
			{
				solutionFolder.Files.Add($"{path}{file.Name}");
			}
		}
		var unwanted = solutionFolder.Files.Where(f => files.All(file => file.Name != f.SlnItemName())).ToList();
		foreach (var file in unwanted)
		{
			solutionFolder.Files.Remove(file);
		}
		foreach (var subDirectory in directory.EnumerateDirectories())
		{
			SyncFolder(solutionFolder, subDirectory, $"{path}{subDirectory.Name}\\");
		}
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

public static class StringExtensions
{
	/// <summary>
	/// Get filename from path in solution items (hard-coded to backslash, so can't use Path.GetFileName)
	/// </summary>
	/// <param name="slnPath"></param>
	/// <returns></returns>
	public static string SlnItemName(this string slnPath) => slnPath.Split('\\').Last();
}
