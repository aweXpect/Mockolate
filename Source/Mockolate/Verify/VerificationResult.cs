using System;
using System.Linq;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     The result of a verification containing the matching interactions.
/// </summary>
public class VerificationResult<TVerify> : IVerificationResult<TVerify>, IVerificationResult
{
	private readonly string _expectation;
	private readonly MockInteractions _interactions;
	private readonly Func<IInteraction, bool> _predicate;
	private readonly TVerify _verify;

	/// <inheritdoc cref="VerificationResult{TMock}" />
	public VerificationResult(TVerify verify,
		MockInteractions interactions,
		Func<IInteraction, bool> predicate,
		string expectation)
	{
		_verify = verify;
		_interactions = interactions;
		_predicate = predicate;
		_expectation = expectation;
	}

	#region IVerificationResult<TVerify>

	/// <inheritdoc cref="IVerificationResult{TVerify}.Object" />
	TVerify IVerificationResult<TVerify>.Object
		=> _verify;

	#endregion

	internal VerificationResult<T> Map<T>(T mock)
		=> new(mock, _interactions, _predicate, _expectation);

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
		IInteraction[] matchingInteractions = _interactions.Interactions.Where(_predicate).ToArray();
		_interactions.Verified(matchingInteractions);
		return predicate(matchingInteractions);
	}

	#endregion
}
