namespace Mockolate.Match;

/// <summary>
///     Matches a method parameter against an expectation.
/// </summary>
public interface IParameter
{
	/// <summary>
	///     Checks if the <paramref name="value"/> matches the expectation.
	/// </summary>
	bool Matches(object? value);
}

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Matches a method parameter of type <typeparamref name="T"/> against an expectation.
/// </summary>
public interface IParameter<out T> : IParameter
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
