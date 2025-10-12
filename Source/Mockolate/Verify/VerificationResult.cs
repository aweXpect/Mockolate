using System;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     The result of a verification containing the matching interactions.
/// </summary>
public class VerificationResult<TVerify> : IVerificationResult<TVerify>
{
	private readonly MockInteractions _interactions;
	private readonly IInteraction[] _matchingInteractions;

	/// <summary>
	///     The verify object for which this expectation applies.
	/// </summary>
	public TVerify Mock { get; }

	/// <inheritdoc cref="VerificationResult{TMock}" />
	public VerificationResult(TVerify mock, MockInteractions interactions, IInteraction[] matchingInteractions, string expectation)
	{
		Mock = mock;
		_interactions = interactions;
		_matchingInteractions = matchingInteractions;
		Expectation = expectation;
	}

	#region IVerificationResult<TVerify>

	/// <inheritdoc cref="IVerificationResult{TVerify}.Expectation" />
	public string Expectation { get; }

	/// <inheritdoc cref="IVerificationResult{TVerify}.Object" />
	TVerify IVerificationResult<TVerify>.Object => Mock;

	/// <inheritdoc cref="IVerificationResult{TVerify}.Verify(Func{IInteraction[], Boolean})" />
	public bool Verify(Func<IInteraction[], bool> predicate)
	{
		_interactions.Verified(_matchingInteractions);
		return predicate(_matchingInteractions);
	}

	#endregion
}
