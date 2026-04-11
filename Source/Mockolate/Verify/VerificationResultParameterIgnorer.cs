using System;
using System.Diagnostics;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     Represents the result of a verification that contains the matching interactions and allows ignoring explicit parameters.
/// </summary>
#if RELEASE
[DebuggerNonUserCode]
#endif
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
