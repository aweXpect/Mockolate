using Mockolate.Checks;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Checks;

public sealed partial class MockAccessedTests
{
	public sealed class ProxyTests
	{
		[Fact]
		public async Task PropertyGetter_ShouldForwardToInner()
		{
			var mockInteractions = new MockInteractions();
			IMockInteractions interactions = mockInteractions;
			var mock = new MyMock<int>(1);
			IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, mock);
			IMockAccessed<Mock<int>> proxy = new MockAccessed<int, Mock<int>>.Proxy(accessed, mockInteractions, mock);
			interactions.RegisterInteraction(new PropertyGetterAccess(0, "foo.bar"));
			interactions.RegisterInteraction(new PropertyGetterAccess(1, "foo.bar"));

			var result1 = accessed.PropertyGetter("foo.bar");
			var result2 = proxy.PropertyGetter("foo.bar");

			await That(result1).Twice();
			await That(result2).Twice();
		}

		[Fact]
		public async Task PropertySetter_ShouldForwardToInner()
		{
			var mockInteractions = new MockInteractions();
			IMockInteractions interactions = mockInteractions;
			var mock = new MyMock<int>(1);
			IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, mock);
			IMockAccessed<Mock<int>> proxy = new MockAccessed<int, Mock<int>>.Proxy(accessed, mockInteractions, mock);
			interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 1));
			interactions.RegisterInteraction(new PropertySetterAccess(1, "foo.bar", 2));

			var result1 = accessed.PropertySetter("foo.bar", With.Any<int>());
			var result2 = proxy.PropertySetter("foo.bar", With.Any<int>());

			await That(result1).Twice();
			await That(result2).Twice();
		}
	}
}
