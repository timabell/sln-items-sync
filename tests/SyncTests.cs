using FluentAssertions;
using NSubstitute;
using sln_items_sync;
using SystemInterface.IO;
using SystemWrapper.IO;
using Xunit.Abstractions;

namespace tests;

public class SyncTests(ITestOutputHelper output)
{
	[Fact]
	public void ModifiesSln()
	{
		const string input = @"
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
";

		var fileFake = Substitute.For<IFile>();
		// fileFake.Exists("original.sln").Returns(true);
		// fileFake.Exists("target.sln").Returns(true);
		fileFake.Exists("rootfile.txt").Returns(true);
		var dirFake = Substitute.For<IDirectory>();
		dirFake.Exists("subfolder").Returns(true);
		// Directory.CreateDirectory(Path.Combine(tmp, "subfolder", "nested_folder"));
		// File.WriteAllText(Path.Combine(tmp, "subfolder", "nested_folder", "nested_file.txt"), "");

		var fakeGuidGenerator = new FakeGuidGenerator();
		fakeGuidGenerator.Guids.Enqueue(new Guid("{17591C35-3F90-4F4A-AA13-45CF8D824066}"));
		fakeGuidGenerator.Guids.Enqueue(new Guid("{CF942CDD-19AC-4E52-9C6E-B1381E0406D9}"));
		fakeGuidGenerator.Guids.Enqueue(new Guid("{F5636E74-888A-4FBD-A8E2-9718A05D90BD}"));

		const string expected = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{2150E333-8FDC-42A3-9474-1A3956D46DE8}"") = ""SolutionItems"", ""SolutionItems"", ""{17591C35-3F90-4F4A-AA13-45CF8D824066}""
	ProjectSection(SolutionItems) = preProject
		rootfile.txt = rootfile.txt
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
";

		var actual = new CLI(fileFake, dirFake, fakeGuidGenerator)
			.SyncSlnText(contents: input, paths: new[] { "rootfile.txt", "subfolder" });

		actual.Should().Be(expected);
	}
}
