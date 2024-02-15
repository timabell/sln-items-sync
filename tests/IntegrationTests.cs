using FluentAssertions;
using sln_items_sync;
using Xunit.Abstractions;

namespace tests;

public class IntegrationTests
{
	private const string TargetSlnFile = "target.sln";
	private readonly CLI _cli;
	private readonly string _testFolder;

	private static readonly string[] GuidsToReturn =
	[
		// deterministic GUID list for tests so we can assert against a fixed expected sln contents
		"17591C35-3F90-4F4A-AA13-45CF8D824066",
		"CF942CDD-19AC-4E52-9C6E-B1381E0406D9",
		"F5636E74-888A-4FBD-A8E2-9718A05D90BD",
		"D6CA39BB-4B2F-4AF7-94B9-D1269AE037D3",
		"5B009B7F-333C-469A-AB3D-24E29C18C544",
	];

	public IntegrationTests(ITestOutputHelper output)
	{
		_cli = new CLI(new FakeGuidGenerator(GuidsToReturn));
		_testFolder = Path.Combine(Path.GetTempPath(), "sln-items-sync-tests", Guid.NewGuid().ToString());
		output.WriteLine($"Test filesystem path:\r\n{_testFolder}");
		Directory.CreateDirectory(_testFolder);
		Directory.SetCurrentDirectory(_testFolder);
	}

	[Fact]
	public void AddsToBlankSln()
	{
		// Arrange
		SetupSln(@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
");

		SetupFilesystem(new[]
		{
			"root-file.txt",
			"subfolder/nested_folder/nested_file.txt",
		});

		// Act
		_cli.Run(["-s", TargetSlnFile, "root-file.txt", "subfolder"]);

		// Assert
		const string expected = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""SolutionItems"", ""SolutionItems"", ""{17591C35-3F90-4F4A-AA13-45CF8D824066}""
	ProjectSection(SolutionItems) = preProject
		root-file.txt = root-file.txt
	EndProjectSection
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""subfolder"", ""subfolder"", ""{CF942CDD-19AC-4E52-9C6E-B1381E0406D9}""
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""nested_folder"", ""nested_folder"", ""{F5636E74-888A-4FBD-A8E2-9718A05D90BD}""
	ProjectSection(SolutionItems) = preProject
		subfolder\nested_folder\nested_file.txt = subfolder\nested_folder\nested_file.txt
	EndProjectSection
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{CF942CDD-19AC-4E52-9C6E-B1381E0406D9} = {17591C35-3F90-4F4A-AA13-45CF8D824066}
		{F5636E74-888A-4FBD-A8E2-9718A05D90BD} = {CF942CDD-19AC-4E52-9C6E-B1381E0406D9}
	EndGlobalSection
EndGlobal
";

		File.WriteAllText(Path.Combine(_testFolder, "expected.sln"), expected); // for kdiff debugging

		ModifiedSln().Should().Be(expected);
	}

	[Fact]
	public void IgnoresExistingItems()
	{
		// Arrange
		SetupSln(@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""SolutionItems"", ""SolutionItems"", ""{17591C35-3F90-4F4A-AA13-45CF8D824066}""
	ProjectSection(SolutionItems) = preProject
		root-file.txt = root-file.txt
	EndProjectSection
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""subfolder"", ""subfolder"", ""{CF942CDD-19AC-4E52-9C6E-B1381E0406D9}""
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""nested_folder"", ""nested_folder"", ""{F5636E74-888A-4FBD-A8E2-9718A05D90BD}""
	ProjectSection(SolutionItems) = preProject
		subfolder\nested_folder\nested_file.txt = subfolder\nested_folder\nested_file.txt
	EndProjectSection
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{CF942CDD-19AC-4E52-9C6E-B1381E0406D9} = {17591C35-3F90-4F4A-AA13-45CF8D824066}
		{F5636E74-888A-4FBD-A8E2-9718A05D90BD} = {CF942CDD-19AC-4E52-9C6E-B1381E0406D9}
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
");

		SetupFilesystem(new[]
		{
			"root-file.txt",
			"subfolder/nested_folder/nested_file.txt",
		});

		// Act
		_cli.Run(["-s", TargetSlnFile, "root-file.txt", "subfolder"]);

		// Assert
		const string expected = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""SolutionItems"", ""SolutionItems"", ""{17591C35-3F90-4F4A-AA13-45CF8D824066}""
	ProjectSection(SolutionItems) = preProject
		root-file.txt = root-file.txt
	EndProjectSection
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""subfolder"", ""subfolder"", ""{CF942CDD-19AC-4E52-9C6E-B1381E0406D9}""
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""nested_folder"", ""nested_folder"", ""{F5636E74-888A-4FBD-A8E2-9718A05D90BD}""
	ProjectSection(SolutionItems) = preProject
		subfolder\nested_folder\nested_file.txt = subfolder\nested_folder\nested_file.txt
	EndProjectSection
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{CF942CDD-19AC-4E52-9C6E-B1381E0406D9} = {17591C35-3F90-4F4A-AA13-45CF8D824066}
		{F5636E74-888A-4FBD-A8E2-9718A05D90BD} = {CF942CDD-19AC-4E52-9C6E-B1381E0406D9}
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
";

		File.WriteAllText(Path.Combine(_testFolder, "expected.sln"), expected); // for kdiff debugging

		ModifiedSln().Should().Be(expected);
	}

	[Fact]
	public void RemovesMissingFiles()
	{
		// Arrange
		SetupSln(@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""SolutionItems"", ""SolutionItems"", ""{17591C35-3F90-4F4A-AA13-45CF8D824066}""
	ProjectSection(SolutionItems) = preProject
		root-file.txt = root-file.txt
	EndProjectSection
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""subfolder"", ""subfolder"", ""{CF942CDD-19AC-4E52-9C6E-B1381E0406D9}""
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""nested_folder"", ""nested_folder"", ""{F5636E74-888A-4FBD-A8E2-9718A05D90BD}""
	ProjectSection(SolutionItems) = preProject
		subfolder\nested_folder\valid_file.txt = subfolder\nested_folder\valid_file.txt
		subfolder\nested_folder\missing_file.txt = subfolder\nested_folder\missing_file.txt
	EndProjectSection
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{CF942CDD-19AC-4E52-9C6E-B1381E0406D9} = {17591C35-3F90-4F4A-AA13-45CF8D824066}
		{F5636E74-888A-4FBD-A8E2-9718A05D90BD} = {CF942CDD-19AC-4E52-9C6E-B1381E0406D9}
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
");

		SetupFilesystem(new[]
		{
			"root-file.txt",
			"subfolder/nested_folder/valid_file.txt",
		});

		// Act
		_cli.Run(["-s", TargetSlnFile, "root-file.txt", "subfolder"]);

		// Assert
		const string expected = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""SolutionItems"", ""SolutionItems"", ""{17591C35-3F90-4F4A-AA13-45CF8D824066}""
	ProjectSection(SolutionItems) = preProject
		root-file.txt = root-file.txt
	EndProjectSection
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""subfolder"", ""subfolder"", ""{CF942CDD-19AC-4E52-9C6E-B1381E0406D9}""
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""nested_folder"", ""nested_folder"", ""{F5636E74-888A-4FBD-A8E2-9718A05D90BD}""
	ProjectSection(SolutionItems) = preProject
		subfolder\nested_folder\valid_file.txt = subfolder\nested_folder\valid_file.txt
	EndProjectSection
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{CF942CDD-19AC-4E52-9C6E-B1381E0406D9} = {17591C35-3F90-4F4A-AA13-45CF8D824066}
		{F5636E74-888A-4FBD-A8E2-9718A05D90BD} = {CF942CDD-19AC-4E52-9C6E-B1381E0406D9}
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
";

		File.WriteAllText(Path.Combine(_testFolder, "expected.sln"), expected); // for kdiff debugging

		ModifiedSln().Should().Be(expected);
	}

	[Fact]
	public void CustomFolderName()
	{
		// Arrange
		SetupSln(@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
");

		SetupFilesystem(new[]
		{
			"root-file.txt",
			"subfolder/nested_folder/nested_file.txt",
		});

		// Act
		_cli.Run(["-s", TargetSlnFile, "-f", "My Items", "root-file.txt", "subfolder"]);

		// Assert
		const string expected = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""My Items"", ""My Items"", ""{17591C35-3F90-4F4A-AA13-45CF8D824066}""
	ProjectSection(SolutionItems) = preProject
		root-file.txt = root-file.txt
	EndProjectSection
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""subfolder"", ""subfolder"", ""{CF942CDD-19AC-4E52-9C6E-B1381E0406D9}""
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""nested_folder"", ""nested_folder"", ""{F5636E74-888A-4FBD-A8E2-9718A05D90BD}""
	ProjectSection(SolutionItems) = preProject
		subfolder\nested_folder\nested_file.txt = subfolder\nested_folder\nested_file.txt
	EndProjectSection
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{CF942CDD-19AC-4E52-9C6E-B1381E0406D9} = {17591C35-3F90-4F4A-AA13-45CF8D824066}
		{F5636E74-888A-4FBD-A8E2-9718A05D90BD} = {CF942CDD-19AC-4E52-9C6E-B1381E0406D9}
	EndGlobalSection
EndGlobal
";
		File.WriteAllText(Path.Combine(_testFolder, "expected.sln"), expected); // for kdiff debugging

		ModifiedSln().Should().Be(expected);
	}

	[Fact]
	public void PathSanitization()
	{
		// Arrange
		SetupSln(@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
");

		SetupFilesystem(new[]
		{
			"root-file.txt",
			"subfolder/nested_file.txt",
		});

		// Act
		_cli.Run(["-s", TargetSlnFile, "root-file.txt", "subfolder/"]);

		// Assert
		const string expected = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""SolutionItems"", ""SolutionItems"", ""{17591C35-3F90-4F4A-AA13-45CF8D824066}""
	ProjectSection(SolutionItems) = preProject
		root-file.txt = root-file.txt
	EndProjectSection
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""subfolder"", ""subfolder"", ""{CF942CDD-19AC-4E52-9C6E-B1381E0406D9}""
	ProjectSection(SolutionItems) = preProject
		subfolder\nested_file.txt = subfolder\nested_file.txt
	EndProjectSection
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{CF942CDD-19AC-4E52-9C6E-B1381E0406D9} = {17591C35-3F90-4F4A-AA13-45CF8D824066}
	EndGlobalSection
EndGlobal
";

		File.WriteAllText(Path.Combine(_testFolder, "expected.sln"), expected); // for kdiff debugging

		ModifiedSln().Should().Be(expected);
	}

	[Fact]
	public void AddsBOM()
	{
		// Arrange
		SetupSln(@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
");

		SetupFilesystem(new[]
		{
			"some-file.txt",
		});

		// Act
		_cli.Run(["-s", TargetSlnFile, "some-file.txt"]);

		File.ReadAllText(Path.Combine(_testFolder, TargetSlnFile));

		// https://stackoverflow.com/questions/5012167/how-can-i-detect-if-a-net-streamreader-found-a-utf8-bom-on-the-underlying-strea/5012344#5012344
		using var fs = new FileStream(Path.Combine(_testFolder, TargetSlnFile), FileMode.Open);
		byte[] bits = new byte[3];
		fs.Read(bits, 0, 3);
		var filePreamble = "0x" + Convert.ToHexString(bits);
		filePreamble.Should().Be("0xEFBBBF");
	}

	[Fact]
	public void RemovesEmptyFolder()
	{
		// Arrange
		SetupSln(@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""SolutionItems"", ""SolutionItems"", ""{80BB2C30-4476-40A5-9362-81EB66FACAD8}""
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""subfolder"", ""subfolder"", ""{96EAD0E2-587F-40D7-8360-460DDCB0EA01}""
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""delete_me"", ""delete_me"", ""{1A8D3B96-F71E-4E09-B64D-65822C9A3FC9}""
	ProjectSection(SolutionItems) = preProject
		subfolder\delete_me\nested_file.txt = subfolder\delete_me\nested_file.txt
	EndProjectSection
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""unchanged_nested_folder"", ""unchanged_nested_folder"", ""{CB69AA4E-8335-4347-B098-8DD404B7B736}""
	ProjectSection(SolutionItems) = preProject
		subfolder\unchanged_nested_folder\nested_file.txt = subfolder\unchanged_nested_folder\nested_file.txt
	EndProjectSection
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{96EAD0E2-587F-40D7-8360-460DDCB0EA01} = {80BB2C30-4476-40A5-9362-81EB66FACAD8}
		{1A8D3B96-F71E-4E09-B64D-65822C9A3FC9} = {96EAD0E2-587F-40D7-8360-460DDCB0EA01}
		{CB69AA4E-8335-4347-B098-8DD404B7B736} = {96EAD0E2-587F-40D7-8360-460DDCB0EA01}
	EndGlobalSection
EndGlobal
");

		SetupFilesystem(new[]
		{
			// "subfolder/delete_me/nested_file.txt", // deleted
			"subfolder/unchanged_nested_folder/nested_file.txt",
		});

		// Act
		_cli.Run(["-s", TargetSlnFile, "subfolder"]);

		// Assert
		const string expected = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""SolutionItems"", ""SolutionItems"", ""{80BB2C30-4476-40A5-9362-81EB66FACAD8}""
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""subfolder"", ""subfolder"", ""{96EAD0E2-587F-40D7-8360-460DDCB0EA01}""
EndProject
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""unchanged_nested_folder"", ""unchanged_nested_folder"", ""{CB69AA4E-8335-4347-B098-8DD404B7B736}""
	ProjectSection(SolutionItems) = preProject
		subfolder\unchanged_nested_folder\nested_file.txt = subfolder\unchanged_nested_folder\nested_file.txt
	EndProjectSection
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
	GlobalSection(NestedProjects) = preSolution
		{96EAD0E2-587F-40D7-8360-460DDCB0EA01} = {80BB2C30-4476-40A5-9362-81EB66FACAD8}
		{CB69AA4E-8335-4347-B098-8DD404B7B736} = {96EAD0E2-587F-40D7-8360-460DDCB0EA01}
	EndGlobalSection
EndGlobal
";

		File.WriteAllText(Path.Combine(_testFolder, "expected.sln"), expected); // for kdiff debugging

		ModifiedSln().Should().Be(expected);
	}

	private string ModifiedSln() => File.ReadAllText(Path.Combine(_testFolder, TargetSlnFile));

	private void SetupFilesystem(IEnumerable<string> paths)
	{
		foreach (var path in paths)
		{
			SetupFile(path);
		}
	}

	private void SetupFile(string path)
	{
		var directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(directory))
		{
			Directory.CreateDirectory(directory);
		}

		File.WriteAllText(Path.Combine(_testFolder, path), contents: "");
	}

	private void SetupSln(string slnContents)
	{
		File.WriteAllText(Path.Combine(_testFolder, "original.sln"), slnContents);
		File.WriteAllText(Path.Combine(_testFolder, TargetSlnFile), slnContents);
	}
}
