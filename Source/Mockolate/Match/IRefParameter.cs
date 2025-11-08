namespace Mockolate.Match;

/// <summary>
///     Matches an <see langword="ref" /> parameter of type <typeparamref name="T"/> against an expectation.
/// </summary>
public interface IRefParameter<T> : IParameter
{
	/// <summary>
	///     Retrieves the value to which the <see langword="out" /> parameter should be set.
	/// </summary>
	T GetValue(T value);
}
