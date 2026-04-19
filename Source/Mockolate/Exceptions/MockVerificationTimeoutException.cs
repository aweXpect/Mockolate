using System;

namespace Mockolate.Exceptions;

/// <summary>
///     Represents a verification timeout error on the mock.
/// </summary>
public class MockVerificationTimeoutException : MockVerificationException
{
	/// <summary>
	///     Creates a new <see cref="MockVerificationTimeoutException" /> that records the elapsed
	///     <paramref name="timeout" /> and wraps the triggering <paramref name="innerException" />.
	/// </summary>
	/// <param name="timeout">The timeout that elapsed, or <see langword="null" /> when the wait was cancelled via a cancellation token.</param>
	/// <param name="innerException">The underlying cancellation or timing exception that triggered this one.</param>
	public MockVerificationTimeoutException(TimeSpan? timeout, Exception innerException)
		: base(timeout is null ? "it timed out" : $"it timed out after {timeout.Value}", innerException)
	{
		Timeout = timeout;
	}

	/// <summary>
	///     The timeout that was reached during verification, if any.
	/// </summary>
	public TimeSpan? Timeout { get; }
}
