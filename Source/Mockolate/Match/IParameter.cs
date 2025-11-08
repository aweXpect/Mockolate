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

/// <summary>
///     Matches a method parameter of type <typeparamref name="T"/> against an expectation.
/// </summary>
public interface IParameter<out T> : IParameter
{
}
