using System;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     The expectation contains the matching interactions for verification.
/// </summary>
public class CheckResult<TMock>
{
	private readonly MockInteractions _interactions;
	private readonly IInteraction[] _matchingInteractions;

	/// <summary>
	///     The expectation of this check result.
	/// </summary>
	public string Expectation { get; }

	/// <summary>
	///     The mock object for which this expectation applies.
	/// </summary>
	public TMock Mock { get; }

	/// <inheritdoc cref="CheckResult{TMock}" />
	public CheckResult(TMock mock, MockInteractions interactions, IInteraction[] matchingInteractions, string expectation)
	{
		Mock = mock;
		_interactions = interactions;
		_matchingInteractions = matchingInteractions;
		Expectation = expectation;
	}

	/// <summary>
	/// Verifies that the specified <paramref name="predicate"/> holds true for the current set of interactions.
	/// </summary>
	public bool Verify(Func<IInteraction[], bool> predicate)
	{
		_interactions.Verified(_matchingInteractions);
		return predicate(_matchingInteractions);
	}
}
