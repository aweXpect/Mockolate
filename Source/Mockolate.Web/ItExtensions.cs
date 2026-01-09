using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Mockolate.Web;

/// <summary>
///     Extensions for parameter matchers for HTTP-related types.
/// </summary>
public static partial class ItExtensions
{
	private static readonly ConcurrentDictionary<string, Regex> RegexCache = new(StringComparer.OrdinalIgnoreCase);

	private static bool MatchesPattern(string value, string pattern)
	{
#if NETFRAMEWORK || NETSTANDARD2_0
		string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
#else
		string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*", StringComparison.Ordinal) + "$";
#endif
		Regex regex = RegexCache.GetOrAdd(regexPattern,
			p => new Regex(p, RegexOptions.IgnoreCase | RegexOptions.Compiled));
		return regex.IsMatch(value);
	}
}
