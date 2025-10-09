using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Interactions;

public sealed class EventUnsubscriptionTests
{
	[Fact]
	public async Task ToString_ShouldReturnExpectedValue()
	{
		EventUnsubscription subscription = new(3, "SomeEvent", this, Helper.GetMethodInfo());
		string expectedValue = "[3] unsubscribe from event SomeEvent";

		await That(subscription.ToString()).IsEqualTo(expectedValue);
	}
}
