using FluentAssertions;
using sln_items_sync;
using Xunit.Abstractions;

namespace tests;

public class IntegrationTests(ITestOutputHelper output)
{
	[Fact]
    public void ModifiesSln()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        output.WriteLine($"Temp path:\r\n{tmp}");
        Directory.CreateDirectory(tmp);
        Directory.SetCurrentDirectory(tmp);
        const string sln = @"

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
        File.WriteAllText(Path.Combine(tmp, "original.sln"), sln);
        File.WriteAllText(Path.Combine(tmp, "target.sln"), sln);
        File.WriteAllText(Path.Combine(tmp, "rootfile.txt"), "");
        Directory.CreateDirectory(Path.Combine(tmp, "subfolder", "nested_folder"));
        File.WriteAllText(Path.Combine(tmp, "subfolder", "nested_folder", "nested_file.txt"), "");

        const string expected = @"

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


        Program.Main(new[] { "-s", "target.sln", "rootfile.txt", "subfolder" });

        var actual = File.ReadAllText(Path.Combine(tmp, "target.sln"));
        actual.Should().Be(expected);
        Directory.GetCurrentDirectory().Should().Be("aaarg");
        // string original = @"something";
        // var fakeFilesystem = FakeFilesystem();

    }
}