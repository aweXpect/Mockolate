#if NETSTANDARD2_0
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Mockolate.Internals.Polyfills;

/// <summary>
///     Provides polyfill extension methods on <see langword="string" />.
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal static class StringExtensionMethods
{
	/// <summary>
	///     Returns a value indicating whether a specified character occurs within this string, using the specified comparison
	///     rules.
	/// </summary>
	/// <returns>
	///     <see langword="true" /> if the <paramref name="value" /> parameter occurs within this string; otherwise,
	///     <see langword="false" />.
	/// </returns>
	internal static bool Contains(
		this string @this,
		string value,
		StringComparison comparisonType)
		=> comparisonType switch
		{
			StringComparison.OrdinalIgnoreCase => @this.ToLowerInvariant().Contains(value.ToLowerInvariant()),
			StringComparison.InvariantCultureIgnoreCase => @this.ToLowerInvariant().Contains(value.ToLowerInvariant()),
			StringComparison.CurrentCultureIgnoreCase => @this.ToLower().Contains(value.ToLower()),
			_ => @this.Contains(value),
		};

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
