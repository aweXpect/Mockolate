using System;
using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches an invocation when the supplied <paramref name="predicate" /> returns <see langword="true" />
	///     for its full argument array.
	/// </summary>
	/// <param name="predicate">
	///     Receives every argument of the invocation, in declaration order, as <see langword="object" />? values.
	///     Ref and out parameters are passed as their current values. Return <see langword="true" /> to accept the
	///     invocation.
	/// </param>
	/// <param name="doNotPopulateThisValue">
	///     Populated by the compiler via <see cref="CallerArgumentExpressionAttribute" /> to include the source
	///     expression of <paramref name="predicate" /> in the setup's <see cref="object.ToString" /> and in
	///     verification failure messages.
	/// </param>
	/// <returns>
	///     An <see cref="IParameters" /> usable in generator-emitted <c>Setup.Method(Match.Parameters(...))</c>
	///     and <c>Verify.Method(Match.Parameters(...))</c> overloads.
	/// </returns>
	/// <remarks>
	///     The Mockolate source generator only emits <see cref="IParameters" />-based <c>Setup</c>/<c>Verify</c>
	///     overloads for methods whose name is unique on the mocked type (no overloads). For overloaded methods,
	///     use per-parameter <see cref="It" /> matchers instead.
	/// </remarks>
	public static IParameters Parameters(Func<object?[], bool> predicate,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue = "")
		=> new ParametersMatch(predicate, doNotPopulateThisValue);

#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class ParametersMatch(Func<object?[], bool> predicate, string predicateExpression) : IParameters, IParametersMatch
	{
		/// <inheritdoc cref="IParametersMatch.Matches" />
		public bool Matches(object?[] values)
			=> predicate(values);

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Match.Parameters({predicateExpression})";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
