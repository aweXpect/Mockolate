using System;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     The result of a verification containing the matching interactions.
/// </summary>
public interface IVerificationResult
{
	/// <summary>
	///     The expectation of this check result.
	/// </summary>
	string Expectation { get; }

	/// <summary>
	///     Gets the complete collection of mock interactions recorded during test execution.
	/// </summary>
	MockInteractions MockInteractions { get; }

	/// <summary>
	/// Verifies that the specified <paramref name="predicate"/> holds true for the current set of interactions.
	/// </summary>
	bool Verify(Func<IInteraction[], bool> predicate);
}

/// <summary>
///     The result of a verification containing the matching interactions.
/// </summary>
public interface IVerificationResult<out TVerify>
{
	/// <summary>
	///     The verify object for which this expectation applies.
	/// </summary>
	TVerify Object { get; }
}
