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
	private readonly IInteraction[] _matchingInteractions;
	private readonly TVerify _verify;

	/// <inheritdoc cref="VerificationResult{TMock}" />
	public VerificationResult(TVerify verify, MockInteractions interactions, IInteraction[] matchingInteractions,
		string expectation)
	{
		_verify = verify;
		_interactions = interactions;
		_matchingInteractions = matchingInteractions;
		_expectation = expectation;
	}

	#region IVerificationResult<TVerify>

	/// <inheritdoc cref="IVerificationResult{TVerify}.Object" />
	TVerify IVerificationResult<TVerify>.Object
		=> _verify;

	#endregion

	internal VerificationResult<T> Map<T>(T mock)
		=> new(mock, _interactions, _matchingInteractions, _expectation);

	#region IVerificationResult

	/// <inheritdoc cref="IVerificationResult.Expectation" />
	string IVerificationResult.Expectation
		=> _expectation;

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
