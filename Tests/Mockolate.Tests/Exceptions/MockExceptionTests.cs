using Mockolate.Exceptions;

namespace Mockolate.Tests.Exceptions;

public sealed class MockExceptionTests
{
	[Fact]
	public async Task ShouldForwardInnerException()
	{
		Exception innerException = new("bar");
		Exception exception = new MockException("foo", innerException);

		await That(exception).HasMessage("foo").And
			.HasInner<Exception>(e => e.HasMessage("bar"));
	}

	[Fact]
	public async Task ShouldForwardMessage()
	{
		Exception exception = new MockException("foo");

		await That(exception).HasMessage("foo");
	}
}
