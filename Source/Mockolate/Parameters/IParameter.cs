namespace Mockolate.Parameters;

/// <summary>
///     Matches a method parameter against an expectation.
/// </summary>
public interface IParameter
{
	/// <summary>
	///     Checks if the <paramref name="value" /> matches the expectation.
	/// </summary>
	/// <remarks>
	///     Used as a covariance-safe fallback when the strongly-typed <see cref="IParameterMatch{T}" /> cast is not
	///     available.
	/// </remarks>
	bool Matches(object? value);

	/// <summary>
	///     Invokes the callbacks registered for this parameter match.
	/// </summary>
	void InvokeCallbacks(object? value);
}

/// <summary>
///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
/// </summary>
/// <remarks>
///     This base contract is callback-free so it can be extended to types that cannot carry
///     an <see cref="System.Action{T}" /> — such as ref struct parameters. For matchers that
///     support <c>.Do(...)</c> callbacks, use <see cref="IParameterWithCallback{T}" />.
/// </remarks>
#pragma warning disable S2326 // Unused type parameters should be removed (T retained for covariance)
public interface IParameter<out T> : IParameter
#if NET9_0_OR_GREATER
	where T : allows ref struct
#endif
	;
#pragma warning restore S2326 // Unused type parameters should be removed
