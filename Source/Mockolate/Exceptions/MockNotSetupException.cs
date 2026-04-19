using System;

namespace Mockolate.Exceptions;

/// <summary>
///     Represents an exception that is thrown when a mock object is used without being properly set up.
/// </summary>
public class MockNotSetupException : MockException
{
	/// <summary>
	///     Creates a new <see cref="MockNotSetupException" /> with the given <paramref name="message" />.
	/// </summary>
	/// <param name="message">Human-readable description of the missing setup.</param>
	public MockNotSetupException(string message) : base(message)
	{
	}

	/// <summary>
	///     Creates a new <see cref="MockNotSetupException" /> with the given <paramref name="message" /> and
	///     <paramref name="innerException" />.
	/// </summary>
	/// <param name="message">Human-readable description of the missing setup.</param>
	/// <param name="innerException">The underlying exception that triggered this one.</param>
	public MockNotSetupException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
