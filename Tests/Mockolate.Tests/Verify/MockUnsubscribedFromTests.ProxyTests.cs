using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockUnsubscribedFromTests
{
	public sealed class ProxyTests
	{
		[Fact]
		public async Task Unsubscribed_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			IMockUnsubscribedFrom<Mock<int>> unsubscribedFrom = new MockUnsubscribedFrom<int, Mock<int>>(verify);
			IMockUnsubscribedFrom<Mock<int>> proxy = new MockUnsubscribedFrom<int, Mock<int>>.Proxy(unsubscribedFrom, verify);
			interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, Helper.GetMethodInfo()));
			interactions.RegisterInteraction(new EventUnsubscription(1, "foo.bar", mock, Helper.GetMethodInfo()));

			CheckResult<Mock<int>> result1 = unsubscribedFrom.Event("foo.bar");
			CheckResult<Mock<int>> result2 = proxy.Event("foo.bar");

			await That(result1.Exactly(2));
			await That(result2.Exactly(2));
		}
	}
}
