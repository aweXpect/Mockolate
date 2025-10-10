using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockGotTests
{
	public sealed class ProxyTests
	{
		[Fact]
		public async Task ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			IMockGot<Mock<int>> mockGot = new MockGot<int, Mock<int>>(verify);
			IMockGot<Mock<int>> proxy = new MockGot<int, Mock<int>>.Proxy(mockGot, verify);
			interactions.RegisterInteraction(new PropertyGetterAccess(0, "foo.bar"));
			interactions.RegisterInteraction(new PropertyGetterAccess(1, "foo.bar"));

			CheckResult<Mock<int>> result1 = mockGot.Property("foo.bar");
			CheckResult<Mock<int>> result2 = proxy.Property("foo.bar");

			await That(result1.Exactly(2));
			await That(result2.Exactly(2));
		}
	}
}
