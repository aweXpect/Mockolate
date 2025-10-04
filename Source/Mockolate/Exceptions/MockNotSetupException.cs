using System;

namespace Mockolate.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a mock object is used without being properly set up.
/// </summary>
public class MockNotSetupException : MockException
{
	/// <inheritdoc cref="MockNotSetupException" />
	public MockNotSetupException(string message) : base(message)
	{
	}

	/// <inheritdoc cref="MockNotSetupException" />
	public MockNotSetupException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
