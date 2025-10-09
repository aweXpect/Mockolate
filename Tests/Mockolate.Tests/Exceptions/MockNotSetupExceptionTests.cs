using Mockolate.Exceptions;

namespace Mockolate.Tests.Exceptions;

public sealed class MockNotSetupExceptionTests
{
	[Fact]
	public async Task ShouldForwardInnerException()
	{
		Exception innerException = new("bar");
		Exception exception = new MockNotSetupException("foo", innerException);

		await That(exception).HasMessage("foo").And
			.HasInner<Exception>(e => e.HasMessage("bar"));
	}

	[Fact]
	public async Task ShouldForwardMessage()
	{
		Exception exception = new MockNotSetupException("foo");

		await That(exception).HasMessage("foo");
	}
}
