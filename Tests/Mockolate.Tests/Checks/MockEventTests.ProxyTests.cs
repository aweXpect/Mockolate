using Mockolate.Checks;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Checks;

public sealed partial class MockEventTests
{
	public sealed class ProxyTests
	{
		/* TODO
		[Fact]
		public async Task Subscribed_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			IMockEvent<Mock<int>> @event = new MockEvent<int, Mock<int>>(mockInteractions, mock);
			IMockEvent<Mock<int>> proxy = new MockEvent<int, Mock<int>>.Proxy(@event, mockInteractions, mock);
			interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, GetMethodInfo()));
			interactions.RegisterInteraction(new EventSubscription(1, "foo.bar", mock, GetMethodInfo()));

			CheckResult<Mock<int>> result1 = @event.Subscribed("foo.bar");
			CheckResult<Mock<int>> result2 = proxy.Subscribed("foo.bar");

			await That(result1).Twice();
			await That(result2).Twice();
		}

		[Fact]
		public async Task Unsubscribed_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			IMockEvent<Mock<int>> @event = new MockEvent<int, Mock<int>>(mockInteractions, mock);
			IMockEvent<Mock<int>> proxy = new MockEvent<int, Mock<int>>.Proxy(@event, mockInteractions, mock);
			interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, GetMethodInfo()));
			interactions.RegisterInteraction(new EventUnsubscription(1, "foo.bar", mock, GetMethodInfo()));

			CheckResult<Mock<int>> result1 = @event.Unsubscribed("foo.bar");
			CheckResult<Mock<int>> result2 = proxy.Unsubscribed("foo.bar");

			await That(result1).Twice();
			await That(result2).Twice();
		}
		*/
	}
}
