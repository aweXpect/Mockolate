using Mockolate.Interactions;

namespace Mockolate.Tests.Interactions;

public sealed class MethodInvocationTests
{
	[Fact]
	public async Task ToString_ShouldReturnExpectedValue()
	{
		MethodInvocation subscription = new(3, "SomeMethod", [1, null, TimeSpan.FromSeconds(90)]);
		string expectedValue = "[3] invoke method SomeMethod(1, null, 00:01:30)";

		await That(subscription.ToString()).IsEqualTo(expectedValue);
	}
}
