namespace sln_items_sync;

public class DefaultGuidGenerator : IGuidGenerator
{
	public Guid Next()
	{
		return Guid.NewGuid();
	}
}
