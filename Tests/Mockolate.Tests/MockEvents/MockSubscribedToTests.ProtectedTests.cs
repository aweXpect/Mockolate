using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockEvents;

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
			MockSubscribedTo<int, Mock<int>> inner = new(verify);
			IMockSubscribedTo<MockVerify<int, Mock<int>>> subscribedTo = inner;
			IMockSubscribedTo<MockVerify<int, Mock<int>>> @protected =
				new ProtectedMockSubscribedTo<int, Mock<int>>(inner);
			interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, Helper.GetMethodInfo()));
			interactions.RegisterInteraction(new EventSubscription(1, "foo.bar", mock, Helper.GetMethodInfo()));

			VerificationResult<MockVerify<int, Mock<int>>> result1 = subscribedTo.Event("foo.bar");
			VerificationResult<MockVerify<int, Mock<int>>> result2 = @protected.Event("foo.bar");

			await That(result1).Exactly(2);
			await That(result2).Exactly(2);
		}
	}
}
