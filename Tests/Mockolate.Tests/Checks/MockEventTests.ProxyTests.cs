using Mockolate.Checks;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using static Mockolate.Checks.CheckResult;

namespace Mockolate.Tests.Checks;

public sealed partial class MockEventTests
{
	public sealed class ProxyTests
	{
		[Fact]
		public async Task Subscribed_ShouldForwardToInner()
		{
			var mockInteractions = new MockInteractions();
			IMockInteractions interactions = mockInteractions;
			var mock = new MyMock<int>(1);
			IMockEvent<Mock<int>> @event = new MockEvent<int, Mock<int>>(mockInteractions, mock);
			IMockEvent<Mock<int>> proxy = new MockEvent<int, Mock<int>>.Proxy(@event, mockInteractions, mock);
			interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, GetMethodInfo()));
			interactions.RegisterInteraction(new EventSubscription(1, "foo.bar", mock, GetMethodInfo()));

			var result1 = @event.Subscribed("foo.bar");
			var result2 = proxy.Subscribed("foo.bar");

			await That(result1).Twice();
			await That(result2).Twice();
		}

		[Fact]
		public async Task Unsubscribed_ShouldForwardToInner()
		{
			var mockInteractions = new MockInteractions();
			IMockInteractions interactions = mockInteractions;
			var mock = new MyMock<int>(1);
			IMockEvent<Mock<int>> @event = new MockEvent<int, Mock<int>>(mockInteractions, mock);
			IMockEvent<Mock<int>> proxy = new MockEvent<int, Mock<int>>.Proxy(@event, mockInteractions, mock);
			interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, GetMethodInfo()));
			interactions.RegisterInteraction(new EventUnsubscription(1, "foo.bar", mock, GetMethodInfo()));

			var result1 = @event.Unsubscribed("foo.bar");
			var result2 = proxy.Unsubscribed("foo.bar");

			await That(result1).Twice();
			await That(result2).Twice();
		}
	}
}
