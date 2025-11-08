namespace Mockolate.Match;

/// <summary>
///     Matches the method parameters against an expectation.
/// </summary>
public interface IParameters
{
	/// <summary>
	///     Checks if the <paramref name="values" /> match the expectations.
	/// </summary>
	public abstract bool Matches(object?[] values);
}
