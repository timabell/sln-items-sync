using SlnParser;

namespace sln_items_sync;

public class Program
{
    public static int Main(string[] args)
    {
        var parser = new SolutionParser();
        var parsedSolution = parser.Parse(args[0]);

        Console.WriteLine("fin");
        return 0;
    }
}