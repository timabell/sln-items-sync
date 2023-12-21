using sln_items_sync;

namespace tests;

public class FakeGuidGenerator : IGuidGenerator
{
	public readonly Queue<Guid> Guids = new();
	public Guid Next()
	{
		return Guids.Dequeue();
	}
}
