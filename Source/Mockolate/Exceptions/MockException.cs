using System;

namespace Mockolate.Exceptions;

/// <summary>
///     Represents the base class for exceptions thrown by mock objects during unit testing.
/// </summary>
public class MockException : Exception
{
	/// <summary>
	///     Creates a new <see cref="MockException" /> with the given <paramref name="message" />.
	/// </summary>
	/// <param name="message">Human-readable description of the failure.</param>
	public MockException(string message) : base(message)
	{
	}

	/// <summary>
	///     Creates a new <see cref="MockException" /> with the given <paramref name="message" /> and
	///     <paramref name="innerException" />.
	/// </summary>
	/// <param name="message">Human-readable description of the failure.</param>
	/// <param name="innerException">The underlying exception that triggered this one.</param>
	public MockException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
