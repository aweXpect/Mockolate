using System;
using System.Threading.Tasks;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     An awaitable <see cref="VerificationResult{TVerify}" /> that uses the timeout or cancellation token to wait for the
///     expected interactions to occur.
/// </summary>
public interface IAsyncVerificationResult : IVerificationResult
{
	/// <summary>
	///     Asynchronously waits until the specified <paramref name="predicate" /> holds true for the current set of
	///     interactions, or until the timeout or cancellation token is triggered.
	/// </summary>
	Task<bool> VerifyAsync(Func<IInteraction[], bool> predicate);
}
