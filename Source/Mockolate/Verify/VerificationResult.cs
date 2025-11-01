using System;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     The result of a verification containing the matching interactions.
/// </summary>
public class VerificationResult<TVerify> : IVerificationResult<TVerify>, IVerificationResult
{
	private readonly MockInteractions _interactions;
	private readonly IInteraction[] _matchingInteractions;
	private readonly TVerify _verify;

	/// <inheritdoc cref="VerificationResult{TMock}" />
	public VerificationResult(TVerify verify, MockInteractions interactions, IInteraction[] matchingInteractions,
		string expectation)
	{
		_verify = verify;
		_interactions = interactions;
		_matchingInteractions = matchingInteractions;
		Expectation = expectation;
	}

	/// <inheritdoc cref="IVerificationResult.Expectation" />
	public string Expectation { get; }

	#region IVerificationResult<TVerify>

	/// <inheritdoc cref="IVerificationResult{TVerify}.Object" />
	TVerify IVerificationResult<TVerify>.Object
		=> _verify;

	#endregion

	/// <inheritdoc cref="IVerificationResult.Verify(Func{IInteraction[], Boolean})" />
	public bool Verify(Func<IInteraction[], bool> predicate)
	{
		_interactions.Verified(_matchingInteractions);
		return predicate(_matchingInteractions);
	}

	#region IVerificationResult

	/// <inheritdoc cref="IVerificationResult.Expectation" />
	string IVerificationResult.Expectation
		=> Expectation;

	/// <inheritdoc cref="IVerificationResult.MockInteractions" />
	MockInteractions IVerificationResult.MockInteractions
		=> _interactions;

	/// <inheritdoc cref="IVerificationResult.Verify(Func{IInteraction[], Boolean})" />
	bool IVerificationResult.Verify(Func<IInteraction[], bool> predicate)
	{
		_interactions.Verified(_matchingInteractions);
		return predicate(_matchingInteractions);
	}

	#endregion
}
