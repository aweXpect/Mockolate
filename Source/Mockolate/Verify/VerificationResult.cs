using System;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     The result of a verification containing the matching interactions.
/// </summary>
public class VerificationResult<TVerify> : IVerificationResult<TVerify>, IVerificationResult
{
	private readonly string _expectation;
	private readonly MockInteractions _interactions;
	private readonly TVerify _verify;
	private readonly IInteraction[] _matchingInteractions;

	/// <inheritdoc cref="VerificationResult{TMock}" />
	public VerificationResult(TVerify verify, MockInteractions interactions, IInteraction[] matchingInteractions, string expectation)
	{
		_verify = verify;
		_interactions = interactions;
		_matchingInteractions = matchingInteractions;
		_expectation = expectation;
	}

	/// <inheritdoc cref="IVerificationResult.Expectation" />
	public string Expectation
		=> _expectation;

	/// <inheritdoc cref="IVerificationResult.Verify(Func{IInteraction[], Boolean})" />
	public bool Verify(Func<IInteraction[], bool> predicate)
	{
		_interactions.Verified(_matchingInteractions);
		return predicate(_matchingInteractions);
	}

	#region IVerificationResult<TVerify>

	/// <inheritdoc cref="IVerificationResult{TVerify}.Expectation" />
	string IVerificationResult<TVerify>.Expectation
		=> _expectation;

	/// <inheritdoc cref="IVerificationResult{TVerify}.Object" />
	TVerify IVerificationResult<TVerify>.Object
		=> _verify;

	/// <inheritdoc cref="IVerificationResult{TVerify}.Verify(Func{IInteraction[], Boolean})" />
	bool IVerificationResult<TVerify>.Verify(Func<IInteraction[], bool> predicate)
	{
		_interactions.Verified(_matchingInteractions);
		return predicate(_matchingInteractions);
	}

	#endregion

	#region IVerificationResult

	/// <inheritdoc cref="IVerificationResult.Expectation" />
	string IVerificationResult.Expectation
		=> _expectation;

	/// <inheritdoc cref="IVerificationResult.Verify(Func{IInteraction[], Boolean})" />
	bool IVerificationResult.Verify(Func<IInteraction[], bool> predicate)
	{
		_interactions.Verified(_matchingInteractions);
		return predicate(_matchingInteractions);
	}

	#endregion
}
