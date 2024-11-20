using System.Runtime.Serialization;
using System.Text;
using SlnEditor.Models;

namespace sln_items_sync;

public class SlnSync(IGuidGenerator guidGenerator)
{
	public SlnSync() : this(new DefaultGuidGenerator())
	{
	}

	/// <summary>
	/// Update "SolutionItems" folder in sln based on filesystem files / folders passed in.
	/// - files will be added if missing
	/// - folders will be forced to recursively match the filesystem
	/// </summary>
	/// <param name="slnPath">relative path to sln file to modify</param>
	/// <param name="slnFolder"></param>
	/// <param name="paths">list of paths to recursively add/update SolutionItems virtual folders with</param>
	public void SyncSlnFile(string? slnPath, string slnFolder, IEnumerable<string> paths)
	{
		if (slnPath is null)
		{
			slnPath = FindSlnFile();
		}
		if (!slnPath.EndsWith(".sln"))
		{
			throw new InvalidSlnPathException(slnPath);
		}
		if (!File.Exists(slnPath))
		{
			throw new SlnFileNotFoundException(slnPath);
		}
		if (!paths.Any())
		{
			throw new EmptyPathListException();
		}
		var contents = File.ReadAllText(slnPath);
		var updatedSln = SyncSlnText(contents, slnFolder, paths);
		File.WriteAllText(slnPath, updatedSln, Encoding.UTF8); // explicit UTF-8 to get byte-order marker (which sln files seem to have)
	}

	public string SyncSlnText(string contents, string slnFolder, IEnumerable<string> paths)
	{
		var solution = new Solution(contents);

		var solutionItems = FindOrCreateSolutionFolder(solution.RootProjects, slnFolder);

		foreach (var path in paths.Select(Path.TrimEndingDirectorySeparator))
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
				throw new PathNotFoundException(path);
			}
		}


		return solution.ToString();
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
		var solutionFolder = FindOrCreateSolutionFolder(parentFolder.Projects, directory.Name);
		var files = directory.GetFiles();
		foreach (var file in files)
		{
			if (solutionFolder.Files.Select<string, string>(f => f.SlnItemName()).All(f => f != file.Name))
			{
				solutionFolder.Files.Add($"{path}{file.Name}");
			}
		}

		var unwanted = solutionFolder.Files
			.Where(f => files.All(file => file.Name != f.SlnItemName())).ToList();

		foreach (var file in unwanted)
		{
			solutionFolder.Files.Remove(file);
		}

		var directories = directory.GetDirectories();
		foreach (var subDirectory in directories)
		{
			SyncFolder(solutionFolder, subDirectory, $"{path}{subDirectory.Name}\\");
		}

		var unwantedFolders = solutionFolder.Projects.OfType<SolutionFolder>()
			.Where(folder => directories.All(d => d.Name != folder.Name)).ToList();

		foreach (var folder in unwantedFolders)
		{
			solutionFolder.Projects.Remove(folder);
		}
	}

	/// <summary>
	/// search current folder for single .sln file and return it
	/// throw if not found or more than one found
	/// </summary>
	/// <returns>sln file if found</returns>
	/// <exception cref="SlnFileNotFoundException"></exception>
	private static string FindSlnFile()
	{
		var slnFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln");
		return slnFiles.Length switch
		{
			0 => throw new SlnFileNotFoundException("No .sln files found in current directory."),
			> 1 => throw new MultipleSlnFilesFoundException("Multiple .sln files found in current directory. Specify one with --solution"),
			_ => slnFiles[0],
		};
	}

	private SolutionFolder FindOrCreateSolutionFolder(ICollection<IProject> solutionProjects,
		string solutionFolderName)
	{
		var solutionItems = FindSolutionFolder(solutionProjects, solutionFolderName);
		if (solutionItems is not null)
		{
			return solutionItems;
		}

		solutionItems = new SolutionFolder(id: guidGenerator.Next(), name: solutionFolderName);
		solutionProjects.Add(solutionItems);

		return solutionItems;
	}

	private static SolutionFolder? FindSolutionFolder(IEnumerable<IProject> solutionProjects, string folderName)
		=> solutionProjects.OfType<SolutionFolder>().FirstOrDefault(project => project.Name == folderName);
}

public class PathNotFoundException(string path) : Exception($"Path not found: '{path}'");
public class MultipleSlnFilesFoundException(string message) : Exception(message: message);
public class SlnFileNotFoundException(string path) : Exception($"Invalid .sln file '{path}' - File not found.");
public class InvalidSlnPathException(string path) : Exception($"Invalid .sln file '{path}' - File must have .sln extension");
public class EmptyPathListException() : Exception();
