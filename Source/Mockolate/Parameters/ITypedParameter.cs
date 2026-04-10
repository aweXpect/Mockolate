namespace Mockolate.Parameters;

/// <summary>
///     Matches a method parameter of type <typeparamref name="T" /> against an expectation
///     using a typed value directly, without boxing through <see cref="INamedParameterValue" />.
/// </summary>
internal interface ITypedParameter<in T>
{
	/// <summary>
	///     Checks if the <paramref name="value" /> matches the expectation.
	/// </summary>
	bool MatchesValue(string name, T value);
}
