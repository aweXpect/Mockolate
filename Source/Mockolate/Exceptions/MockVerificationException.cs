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
}
