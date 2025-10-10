using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockInvokedTests
{
	public sealed class ProxyTests
	{
		[Fact]
		public async Task PropertySetter_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			IMockInvoked<Mock<int>> invoked = new MockInvoked<int, Mock<int>>(verify);
			IMockInvoked<Mock<int>> proxy = new MockInvoked<int, Mock<int>>.Proxy(invoked, verify);
			interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [1,]));
			interactions.RegisterInteraction(new MethodInvocation(1, "foo.bar", [2,]));

			VerificationResult<Mock<int>> result1 = invoked.Method("foo.bar", With.Any<int>());
			VerificationResult<Mock<int>> result2 = proxy.Method("foo.bar", With.Any<int>());

			await That(result1.Exactly(2));
			await That(result2.Exactly(2));
		}
	}
}
