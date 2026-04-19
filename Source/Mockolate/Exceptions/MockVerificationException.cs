using System;

namespace Mockolate.Exceptions;

/// <summary>
///     Represents a verification error on the mock.
/// </summary>
public class MockVerificationException : MockException
{
	/// <summary>
	///     Creates a new <see cref="MockVerificationException" /> with the given <paramref name="message" />.
	/// </summary>
	/// <param name="message">Human-readable description of the verification failure.</param>
	public MockVerificationException(string message) : base(message)
	{
	}

	/// <summary>
	///     Creates a new <see cref="MockVerificationException" /> with the given <paramref name="message" /> and
	///     <paramref name="innerException" />.
	/// </summary>
	/// <param name="message">Human-readable description of the verification failure.</param>
	/// <param name="innerException">The underlying exception that triggered this one.</param>
	public MockVerificationException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
