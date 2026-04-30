#if NETSTANDARD2_0
using System.Diagnostics.CodeAnalysis;

namespace Mockolate.Internals.Polyfills;

/// <summary>
///     Provides polyfill extension methods on <see langword="string" />.
/// </summary>
[ExcludeFromCodeCoverage]
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
internal static class StringExtensionMethods
{
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
