using System;
using System.Diagnostics;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     The result of a verification containing the matching interactions, that allows ignoring the explicit parameters.
/// </summary>
[DebuggerNonUserCode]
public class VerificationResultParameterIgnorer<TVerify> : VerificationResult<TVerify>
{
	private readonly Func<VerificationResult<TVerify>> _anyParametersFactory;

	internal VerificationResultParameterIgnorer(
		TVerify verify,
		MockInteractions interactions,
		Func<IInteraction, bool> predicate,
		Func<string> expectation,
		Func<VerificationResult<TVerify>> anyParametersFactory)
		: base(verify, interactions, predicate, expectation)
	{
		_anyParametersFactory = anyParametersFactory;
	}

	/// <summary>
	///     Replaces the explicit parameter matcher with <see cref="Match.AnyParameters()" />.
	/// </summary>
	public VerificationResult<TVerify> AnyParameters() => _anyParametersFactory();
}
