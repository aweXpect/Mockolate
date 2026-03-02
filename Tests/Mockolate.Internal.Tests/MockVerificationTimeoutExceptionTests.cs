using Mockolate.Exceptions;

namespace Mockolate.Internal.Tests;

public class MockVerificationTimeoutExceptionTests
{
	[Fact]
	public async Task WithoutTimeout_ShouldHaveTimedOutMessage()
	{
		Exception exception = new("foo");
		MockVerificationTimeoutException sut = new(null, exception);

		await That(sut.Timeout).IsNull();
		await That(sut.Message).IsEqualTo("it timed out");
		await That(sut.InnerException).IsSameAs(exception);
	}

	[Fact]
	public async Task WithTimeout_ShouldIncludeTimeoutInMessage()
	{
		Exception exception = new("foo");
		MockVerificationTimeoutException sut = new(TimeSpan.FromSeconds(30), exception);

		await That(sut.Timeout).IsEqualTo(TimeSpan.FromSeconds(30));
		await That(sut.Message).IsEqualTo("it timed out after 00:00:30");
		await That(sut.InnerException).IsSameAs(exception);
	}
}
