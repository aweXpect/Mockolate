using System;

namespace Mockolate.Parameters;

/// <summary>
///     Matches the method parameters against an expectation.
/// </summary>
public interface INamedParametersMatch
{
	/// <summary>
	///     Checks if the <paramref name="values" /> match the expectations.
	/// </summary>
	bool Matches(ReadOnlySpan<(string, object?)> values);
}
