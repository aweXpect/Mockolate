using System;
using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches the parameters against the <paramref name="predicate"/>.
	/// </summary>
	public static IParameters Parameters(Func<(string? Name, object? Value)[], bool> predicate,
		[CallerArgumentExpression("predicate")] string doNotPopulateThisValue = "")
		=> new ParametersMatch(predicate, doNotPopulateThisValue);

	private sealed class ParametersMatch(Func<(string? Name, object? Value)[], bool> predicate, string predicateExpression) : IParameters
	{
		/// <inheritdoc cref="IParameters.Matches" />
		public bool Matches((string? Name, object? Value)[] values)
			=> predicate(values);

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Match.Parameters({predicateExpression})";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
