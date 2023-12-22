using sln_items_sync;

namespace tests;

public class FakeGuidGenerator : IGuidGenerator
{
	public FakeGuidGenerator(IEnumerable<string> guidsToReturn)
	{
		foreach (var guid in guidsToReturn)
		{
			_guids.Enqueue(new Guid(guid));
		}
	}

	private readonly Queue<Guid> _guids = new();

	public Guid Next()
	{
		return _guids.Dequeue();
	}
}
