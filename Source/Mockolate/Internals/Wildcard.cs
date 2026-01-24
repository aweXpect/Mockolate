using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Mockolate.Internals;

internal readonly struct Wildcard
{
	public Regex Regex { get; }

	private static readonly ConcurrentDictionary<(string RegexPattern, bool IgnoreCase), Wildcard> RegexCache = new();

	public static Wildcard Pattern(string pattern, bool ignoreCase)
	{
		Wildcard wildcard = RegexCache.GetOrAdd((ToRegularExpression(pattern), ignoreCase),
			item => new Wildcard(
				new Regex(
					item.RegexPattern,
					item.IgnoreCase
						? RegexOptions.IgnoreCase | RegexOptions.Compiled
						: RegexOptions.Compiled,
					Regex.InfiniteMatchTimeout)));
		return wildcard;
	}

	private Wildcard(Regex regex)
	{
		Regex = regex;
	}

	public bool Matches(string value)
		=> Regex.IsMatch(value);

	public static string ToRegularExpression(string pattern)
	{
		string regex = Regex.Escape(pattern)
#if NETFRAMEWORK || NETSTANDARD2_0
			.Replace("\\?", ".")
			.Replace("\\*", "(?:.|\\n)*");
#else
			.Replace("\\?", ".", StringComparison.Ordinal)
			.Replace("\\*", "(?:.|\\n)*", StringComparison.Ordinal);
#endif
		return $"^{regex}$";
	}
}
