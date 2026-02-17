using CommandLine;

namespace sln_items_sync;

public class CLI
{
	private readonly SlnSync _slnSync;

	public CLI()
	{
		_slnSync = new SlnSync();
	}

	public CLI(IGuidGenerator guidGenerator)
	{
		_slnSync = new SlnSync(guidGenerator);
	}

	private class Options
	{
		[Option('s', "solution", HelpText = "path to .sln file to modify")]
		public string? SlnPath { get; set; }

		[Option('f', "folder", Required = false, HelpText = "Solution folder to target")]
		public string SlnFolder { get; set; } = "Solution Items";

		[Value(0)]
		public required IEnumerable<string> Paths { get; set; }
	}

	public int Run(string[] args)
	{
		Console.Error.WriteLine("https://github.com/timabell/sln-items-sync");
		Console.Error.WriteLine("https://www.nuget.org/packages/sln-items-sync");
		Console.Error.WriteLine("A-GPL v3 Licensed");
		Console.Error.WriteLine();
		var parserResult = Parser.Default.ParseArguments<Options>(args);
		if (parserResult.Errors.Any())
		{
			WriteUsage();
			return 1;
		}

		try
		{
			parserResult
				.WithParsed(opts =>
					_slnSync.SyncSlnFile(slnPath: opts.SlnPath, slnFolder: opts.SlnFolder, opts.Paths));
		} catch(InvalidSlnPathException ex) {
			Console.Error.WriteLine(ex.Message);
			return 2;
		} catch(SlnFileNotFoundException ex) {
			Console.Error.WriteLine(ex.Message);
			return 3;
		} catch(PathNotFoundException ex) {
			Console.Error.WriteLine(ex.Message);
			return 4;
		} catch(EmptyPathListException) {
			Console.Error.WriteLine("No paths supplied to sync with.");
			Console.Error.WriteLine("Supply a list of folder/file arguments that you want to be sync'd into the target solution folder.");
			Console.Error.WriteLine();
			WriteUsage();
			return 5;
		} catch(MultipleSlnFilesFoundException ex) {
			Console.Error.WriteLine(ex.Message);
			return 6;
		}

		Console.Out.WriteLine("Sync completed.");
		return 0;
	}

	private static void WriteUsage()
	{
		Console.Error.WriteLine("Usage: sln-items-sync --solution <SolutionFile.sln> paths-to-sync ...");
	}
}
