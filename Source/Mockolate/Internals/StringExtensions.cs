namespace Mockolate.Internals;

internal static class StringExtensions
{
	internal static string SubstringUntilFirst(this string name, char c)
	{
		int index = name.IndexOf(c);
		if (index >= 0)
		{
			return name.Substring(0, index);
		}

		return name;
	}

	internal static string SubstringAfterLast(this string name, char c)
	{
		int index = name.LastIndexOf(c);
		if (index >= 0)
		{
			return name.Substring(index + 1);
		}

		return name;
	}
}
