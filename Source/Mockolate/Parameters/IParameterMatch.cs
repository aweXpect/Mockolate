namespace Mockolate.Parameters;

/// <summary>
///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
/// </summary>
public interface IParameterMatch<in T>
{
	/// <summary>
	///     Checks if the <paramref name="value" /> matches the expectation.
	/// </summary>
	bool Matches(T value);

	/// <summary>
	///     Invokes the callbacks registered for this parameter match.
	/// </summary>
	void InvokeCallbacks(T value);
}
