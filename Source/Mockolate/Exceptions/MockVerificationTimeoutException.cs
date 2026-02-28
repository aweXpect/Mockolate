using System;

namespace Mockolate.Exceptions;

/// <summary>
///     Represents a verification error on the mock.
/// </summary>
internal class MockVerificationTimeoutException : MockException
{
	/// <inheritdoc cref="MockVerificationException" />
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
