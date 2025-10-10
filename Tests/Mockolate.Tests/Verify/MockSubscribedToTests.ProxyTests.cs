using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockSubscribedToTests
{
	public sealed class ProxyTests
	{
		[Fact]
		public async Task Subscribed_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			IMockSubscribedTo<Mock<int>> subscribedTo = new MockSubscribedTo<int, Mock<int>>(verify);
			IMockSubscribedTo<Mock<int>> proxy = new MockSubscribedTo<int, Mock<int>>.Proxy(subscribedTo, verify);
			interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, Helper.GetMethodInfo()));
			interactions.RegisterInteraction(new EventSubscription(1, "foo.bar", mock, Helper.GetMethodInfo()));

			CheckResult<Mock<int>> result1 = subscribedTo.Event("foo.bar");
			CheckResult<Mock<int>> result2 = proxy.Event("foo.bar");

			await That(result1.Exactly(2));
			await That(result2.Exactly(2));
		}
	}
}
