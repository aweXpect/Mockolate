namespace Mockolate.Parameters;

/// <summary>
///     Matches the method parameters against an expectation.
/// </summary>
public interface IParameters
{
	/// <summary>
	///     Checks if the <paramref name="values" /> match the expectations.
	/// </summary>
	bool Matches(NamedParameterValue[] values);
}
