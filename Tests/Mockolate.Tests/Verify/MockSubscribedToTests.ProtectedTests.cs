using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockSubscribedToTests
{
	public sealed class ProtectedTests
	{
		[Fact]
		public async Task Subscribed_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			IMockSubscribedTo<Mock<int>> subscribedTo = new MockSubscribedTo<int, Mock<int>>(verify);
			IMockSubscribedTo<Mock<int>> @protected = new MockSubscribedTo<int, Mock<int>>.Protected(verify);
			interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, Helper.GetMethodInfo()));
			interactions.RegisterInteraction(new EventSubscription(1, "foo.bar", mock, Helper.GetMethodInfo()));

			VerificationResult<Mock<int>> result1 = subscribedTo.Event("foo.bar");
			VerificationResult<Mock<int>> result2 = @protected.Event("foo.bar");

			result1.Exactly(2);
			result2.Exactly(2);
		}
	}
}
