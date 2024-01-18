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
		[Option('s', "solution", Required = true, HelpText = "path to .sln file to modify")]
		public string SlnPath { get; set; }

		[Option('f', "folder", Required = false, HelpText = "Solution folder to target")]
		public string SlnFolder { get; set; } = "SolutionItems";

		[Value(0)]
		public IEnumerable<string> Paths { get; set; }
	}

	public int Run(string[] args)
	{
		Console.Out.WriteLine("https://github.com/timabell/sln-items-sync");
		Console.Out.WriteLine("https://www.nuget.org/packages/sln-items-sync");
		Console.Out.WriteLine("A-GPL v3 Licensed");
		var parserResult = Parser.Default.ParseArguments<Options>(args);
		if (parserResult.Errors.Any())
		{
			return 1;
		}

		parserResult
			.WithParsed(opts =>
				_slnSync.SyncSlnFile(slnPath: opts.SlnPath, slnFolder: opts.SlnFolder, opts.Paths));

		return 0;
	}
}
