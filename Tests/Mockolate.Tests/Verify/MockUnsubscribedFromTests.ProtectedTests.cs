using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockUnsubscribedFromTests
{
	public sealed class ProtectedTests
	{
		[Fact]
		public async Task Unsubscribed_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			IMockUnsubscribedFrom<Mock<int>> unsubscribedFrom = new MockUnsubscribedFrom<int, Mock<int>>(verify);
			IMockUnsubscribedFrom<Mock<int>> @protected = new MockUnsubscribedFrom<int, Mock<int>>.Protected(verify);
			interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, Helper.GetMethodInfo()));
			interactions.RegisterInteraction(new EventUnsubscription(1, "foo.bar", mock, Helper.GetMethodInfo()));

			VerificationResult<Mock<int>> result1 = unsubscribedFrom.Event("foo.bar");
			VerificationResult<Mock<int>> result2 = @protected.Event("foo.bar");

			await That(result1).Exactly(2);
			await That(result2).Exactly(2);
		}
	}
}
