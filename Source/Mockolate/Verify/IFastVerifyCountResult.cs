using System;

namespace Mockolate.Verify;

/// <summary>
///     Internal fast-path contract: a verification result that can answer count terminators
///     (<c>Once</c>, <c>Exactly</c>, <c>AtLeast</c>, ...) without materialising the matching
///     <see cref="Mockolate.Interactions.IInteraction" /> array. Implemented by
///     <see cref="VerificationResult{TVerify}" /> (and its awaitable variant) so the count
///     extensions can skip the fallback through <see cref="IVerificationResult.Verify" />.
/// </summary>
internal interface IFastVerifyCountResult
{
	/// <summary>
	///     Counts the matching interactions and returns whether <paramref name="countPredicate" />
	///     accepts the count. Implementations route through the typed per-member buffer's
	///     <c>CountMatching</c> when available; otherwise fall back to materialising and counting.
	/// </summary>
	bool VerifyCount(Func<int, bool> countPredicate);
}
