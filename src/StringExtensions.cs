namespace sln_items_sync;

public static class StringExtensions
{
	/// <summary>
	/// Get filename from path in solution items (hard-coded to backslash, so can't use Path.GetFileName)
	/// </summary>
	/// <param name="slnPath"></param>
	/// <returns></returns>
	public static string SlnItemName(this string slnPath) => slnPath.Split('\\').Last();
}
