#if NETSTANDARD2_0
using System.Diagnostics.CodeAnalysis;

namespace Mockolate.Internals.Polyfills;

/// <summary>
///     Provides extension methods to simplify writing platform independent tests.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class StringExtensionMethods
{
	/// <summary>
	///     Determines whether the end of this string instance matches the specified character.
	/// </summary>
	internal static bool EndsWith(
		this string @this,
		char value)
		=> @this.EndsWith($"{value}");

	/// <summary>
	///     Splits a string into a maximum number of substrings based on the provided character <paramref name="separator" />,
	///     optionally omitting empty substrings from the result.
	/// </summary>
	internal static string[] Split(
		this string @this,
		char separator,
		int count)
		=> @this.Split([separator,], count);
}
#endif
