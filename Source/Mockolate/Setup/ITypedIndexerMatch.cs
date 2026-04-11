namespace Mockolate.Setup;

/// <summary>
///     Matches an indexer access by typed parameters, without boxing through
///     <see cref="Mockolate.Parameters.INamedParameterValue" />.
/// </summary>
internal interface ITypedIndexerMatch
{
	/// <summary>
	///     Checks if the single parameter value matches.
	/// </summary>
	bool MatchesTyped<T1>(string n1, T1 v1);

	/// <summary>
	///     Checks if the two parameter values match.
	/// </summary>
	bool MatchesTyped<T1, T2>(string n1, T1 v1, string n2, T2 v2);

	/// <summary>
	///     Checks if the three parameter values match.
	/// </summary>
	bool MatchesTyped<T1, T2, T3>(string n1, T1 v1, string n2, T2 v2, string n3, T3 v3);

#pragma warning disable S107 // Methods should not have too many parameters
	/// <summary>
	///     Checks if the four parameter values match.
	/// </summary>
	bool MatchesTyped<T1, T2, T3, T4>(string n1, T1 v1, string n2, T2 v2, string n3, T3 v3,
		string n4, T4 v4);
#pragma warning restore S107 // Methods should not have too many parameters
}
