using Mockolate.Checks;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Checks;

public sealed partial class MockAccessedTests
{
	public sealed class ProtectedTests
	{
		/* TODO
		[Fact]
		public async Task PropertyGetter_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, mock);
			IMockAccessed<Mock<int>> @protected =
				new MockAccessed<int, Mock<int>>.Protected(accessed, mockInteractions, mock);
			interactions.RegisterInteraction(new PropertyGetterAccess(0, "foo.bar"));
			interactions.RegisterInteraction(new PropertyGetterAccess(1, "foo.bar"));

			CheckResult<Mock<int>> result1 = accessed.PropertyGetter("foo.bar");
			CheckResult<Mock<int>> result2 = @protected.PropertyGetter("foo.bar");

			await That(result1).Twice();
			await That(result2).Twice();
		}

		[Fact]
		public async Task PropertySetter_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, mock);
			IMockAccessed<Mock<int>> @protected =
				new MockAccessed<int, Mock<int>>.Protected(accessed, mockInteractions, mock);
			interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 1));
			interactions.RegisterInteraction(new PropertySetterAccess(1, "foo.bar", 2));

			CheckResult<Mock<int>> result1 = accessed.PropertySetter("foo.bar", With.Any<int>());
			CheckResult<Mock<int>> result2 = @protected.PropertySetter("foo.bar", With.Any<int>());

			await That(result1).Twice();
			await That(result2).Twice();
		}
		*/
	}
}
