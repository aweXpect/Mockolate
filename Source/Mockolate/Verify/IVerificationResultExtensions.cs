using System;

namespace Mockolate.Verify;

internal static class IVerificationResultExtensions
{
	/// <summary>
	///     Routes count terminators to the allocation-free fast path when the result implements
	///     <see cref="IFastVerifyCountResult" />; otherwise falls back to materialising the matching
	///     interactions through <see cref="IVerificationResult.Verify" /> and counting them. Lets
	///     external implementers of <see cref="IVerificationResult" /> remain whole-interface
	///     implementable while still letting the framework's own results take the fast path.
	/// </summary>
	internal static bool VerifyCount(this IVerificationResult result, Func<int, bool> countPredicate)
		=> result is IFastVerifyCountResult fast
			? fast.VerifyCount(countPredicate)
			: result.Verify(arr => countPredicate(arr.Length));
}
