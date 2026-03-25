using System;

namespace Mockolate.Exceptions;

/// <summary>
///     Represents a verification error on the mock.
/// </summary>
public class MockVerificationException : MockException
{
	/// <inheritdoc cref="MockVerificationException" />
	public MockVerificationException(string message) : base(message)
	{
	}

	/// <inheritdoc cref="MockVerificationException" />
	public MockVerificationException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
