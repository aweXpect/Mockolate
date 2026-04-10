namespace Mockolate.Setup;

/// <summary>
///     Matches a method call by name and typed parameters, without boxing through
///     <see cref="Mockolate.Parameters.INamedParameterValue" />.
/// </summary>
internal interface ITypedMethodMatch
{
	/// <summary>
	///     Checks if the method name and single parameter value match.
	/// </summary>
	bool MatchesTyped<T1>(string methodName, string n1, T1 v1);

	/// <summary>
	///     Checks if the method name and two parameter values match.
	/// </summary>
	bool MatchesTyped<T1, T2>(string methodName, string n1, T1 v1, string n2, T2 v2);

	/// <summary>
	///     Checks if the method name and three parameter values match.
	/// </summary>
	bool MatchesTyped<T1, T2, T3>(string methodName, string n1, T1 v1, string n2, T2 v2, string n3, T3 v3);

	/// <summary>
	///     Checks if the method name and four parameter values match.
	/// </summary>
	bool MatchesTyped<T1, T2, T3, T4>(string methodName, string n1, T1 v1, string n2, T2 v2, string n3, T3 v3,
		string n4, T4 v4);
}
