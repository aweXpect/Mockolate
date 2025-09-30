using System;

namespace Mockerade.Exceptions;

/// <summary>
///     Represents the base class for exceptions thrown by mock objects during unit testing.
/// </summary>
public class MockException : Exception
{
	/// <inheritdoc cref="MockException" />
	public MockException(string message) : base(message)
	{
	}

	/// <inheritdoc cref="MockException" />
	public MockException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
