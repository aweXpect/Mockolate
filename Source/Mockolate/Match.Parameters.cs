using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches the parameters against the <paramref name="predicate" />.
	/// </summary>
	public static IParameters Parameters(Func<object?[], bool> predicate,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue = "")
		=> new ParametersMatch(predicate, doNotPopulateThisValue);
	/// <summary>
	///     Matches the parameters against the <paramref name="predicate" />.
	/// </summary>
	public static IParameters Parameters(Func<(string, object?)[], bool> predicate,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue = "")
		=> new NamedParametersMatch(predicate, doNotPopulateThisValue);

#if !DEBUG
	[DebuggerNonUserCode]
#endif
	private sealed class ParametersMatch(Func<object?[], bool> predicate, string predicateExpression) : IParameters, IParametersMatch
	{
		/// <inheritdoc cref="IParametersMatch.Matches" />
		public bool Matches(object?[] values)
			=> predicate(values);

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Match.Parameters({predicateExpression})";
	}

#if !DEBUG
	[DebuggerNonUserCode]
#endif
	private sealed class NamedParametersMatch(Func<(string, object?)[], bool> predicate, string predicateExpression) : IParameters, INamedParametersMatch
	{
		/// <inheritdoc cref="INamedParametersMatch.Matches" />
		public bool Matches((string, object?)[] values)
			=> predicate(values);

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Match.Parameters({predicateExpression})";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
