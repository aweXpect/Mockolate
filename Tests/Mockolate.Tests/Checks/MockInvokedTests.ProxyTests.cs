using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Checks;

public sealed partial class MockInvokedTests
{
	public sealed class ProxyTests
	{
		/* TODO
		[Fact]
		public async Task PropertySetter_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			IMockInvoked<Mock<int>> invoked = new MockInvoked<int, Mock<int>>(mockInteractions, mock);
			IMockInvoked<Mock<int>> proxy = new MockInvoked<int, Mock<int>>.Proxy(invoked, mockInteractions, mock);
			interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [1,]));
			interactions.RegisterInteraction(new MethodInvocation(1, "foo.bar", [2,]));

			CheckResult<Mock<int>> result1 = invoked.Method("foo.bar", With.Any<int>());
			CheckResult<Mock<int>> result2 = proxy.Method("foo.bar", With.Any<int>());

			await That(result1).Twice();
			await That(result2).Twice();
		} */
	}
}
