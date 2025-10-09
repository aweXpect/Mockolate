using Mockolate.Interactions;

namespace Mockolate.Tests.Interactions;

public sealed class PropertyGetterAccessTests
{
	[Fact]
	public async Task ToString_ShouldReturnExpectedValue()
	{
		PropertyGetterAccess subscription = new(3, "SomeProperty");
		string expectedValue = "[3] get property SomeProperty";

		await That(subscription.ToString()).IsEqualTo(expectedValue);
	}
}
