using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockSetTests
{
	public sealed class ProtectedTests
	{
		[Fact]
		public async Task ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			IMockSet<Mock<int>> mockSet = new MockSet<int, Mock<int>>(verify);
			IMockSet<Mock<int>> @protected = new MockSet<int, Mock<int>>.Protected(verify);
			interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 1));
			interactions.RegisterInteraction(new PropertySetterAccess(1, "foo.bar", 2));

			CheckResult<Mock<int>> result1 = mockSet.Property("foo.bar", With.Any<int>());
			CheckResult<Mock<int>> result2 = @protected.Property("foo.bar", With.Any<int>());

			await That(result1.Exactly(2));
			await That(result2.Exactly(2));
		}
	}
}
