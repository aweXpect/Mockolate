namespace Mockolate.Match;

/// <summary>
///     Matches an <see langword="out" /> parameter of type <typeparamref name="T"/> against an expectation.
/// </summary>
public interface IOutParameter<out T> : IParameter
{
	/// <summary>
	///     Retrieves the value to which the <see langword="out" /> parameter should be set.
	/// </summary>
	T GetValue();
}
