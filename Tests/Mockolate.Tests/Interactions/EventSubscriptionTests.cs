using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Interactions;

public sealed class EventSubscriptionTests
{
	[Fact]
	public async Task ToString_ShouldReturnExpectedValue()
	{
		EventSubscription subscription = new(3, "SomeEvent", this, Helper.GetMethodInfo());
		string expectedValue = "[3] subscribe to event SomeEvent";

		await That(subscription.ToString()).IsEqualTo(expectedValue);
	}
}
