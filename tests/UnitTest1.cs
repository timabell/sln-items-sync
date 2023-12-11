using FluentAssertions;
using sln_items_sync;
using Xunit.Abstractions;

namespace tests;

public class UnitTest1(ITestOutputHelper output)
{
	[Fact]
    public void Test1()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        output.WriteLine($"Temp path {tmp}");
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
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
";
        File.WriteAllText(Path.Combine(tmp, "original.sln"), sln);
        File.WriteAllText(Path.Combine(tmp, "target.sln"), sln);
        File.WriteAllText(Path.Combine(tmp, "somefile.txt"), "");
        Directory.CreateDirectory(Path.Combine(tmp, "somefolder"));
        Directory.CreateDirectory(Path.Combine(tmp, "somefolder", "subfolder"));
        File.WriteAllText(Path.Combine(tmp, "somefolder", "subfolder", "file.txt"), "");

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
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
";


        Program.Main(new[] { "-s", "target.sln", "somefile.txt", "somefolder" });

        var actual = File.ReadAllText(Path.Combine(tmp, "target.sln"));
        actual.Should().Be(expected);
        Directory.GetCurrentDirectory().Should().Be("aaarg");
        // string original = @"something";
        // var fakeFilesystem = FakeFilesystem();

    }
}