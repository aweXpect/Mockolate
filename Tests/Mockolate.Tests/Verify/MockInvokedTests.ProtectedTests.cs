using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockInvokedTests
{
	public sealed class ProtectedTests
	{
		[Fact]
		public async Task PropertySetter_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			MockInvoked<int, Mock<int>> inner = new(verify);
			IMockInvoked<MockVerify<int, Mock<int>>> invoked = inner;
			IMockInvoked<MockVerify<int, Mock<int>>> @protected = new ProtectedMockInvoked<int, Mock<int>>(inner);
			interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [1,]));
			interactions.RegisterInteraction(new MethodInvocation(1, "foo.bar", [2,]));

			VerificationResult<MockVerify<int, Mock<int>>> result1 = invoked.Method("foo.bar", With.Any<int>());
			VerificationResult<MockVerify<int, Mock<int>>> result2 = @protected.Method("foo.bar", With.Any<int>());

			await That(result1).Exactly(2);
			await That(result2).Exactly(2);
		}
	}
}
