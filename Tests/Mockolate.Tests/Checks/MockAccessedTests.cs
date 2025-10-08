using Mockolate.Checks;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Checks;

public sealed partial class MockAccessedTests
{
	[Fact]
	public async Task PropertyGetter_WithoutInteractions_ShouldReturnNeverResult()
	{
		var interactions = new MockInteractions();
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(interactions, new MyMock<int>(1));

		var result = accessed.PropertyGetter("foo.bar");

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("accessed getter of property bar");
	}

	[Fact]
	public async Task PropertyGetter_WhenNameMatches_ShouldReturnOnce()
	{
		var mockInteractions = new MockInteractions();
		IMockInteractions interactions = mockInteractions;
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new PropertyGetterAccess(0, "foo.bar"));

		var result = accessed.PropertyGetter("foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task PropertyGetter_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		var mockInteractions = new MockInteractions();
		IMockInteractions interactions = mockInteractions;
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new PropertyGetterAccess(0, "foo.bar"));

		var result = accessed.PropertyGetter("baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task PropertySetter_WhenOnlyValueMatches_ShouldReturnNever()
	{
		var mockInteractions = new MockInteractions();
		IMockInteractions interactions = mockInteractions;
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		var result = accessed.PropertySetter("baz.bar", With.Any<int>());

		await That(result).Never();
	}

	[Fact]
	public async Task PropertySetter_WithoutInteractions_ShouldReturnNeverResult()
	{
		var interactions = new MockInteractions();
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(interactions, new MyMock<int>(1));

		var result = accessed.PropertySetter("foo.bar", With.Any<int>());

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("accessed setter of property bar with value With.Any<int>()");
	}

	[Fact]
	public async Task PropertySetter_WhenNameAndValueMatches_ShouldReturnOnce()
	{
		var mockInteractions = new MockInteractions();
		IMockInteractions interactions = mockInteractions;
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		var result = accessed.PropertySetter("foo.bar", With.Any<int>());

		await That(result).Once();
	}

	[Fact]
	public async Task PropertySetter_WhenOnlyNameMatches_ShouldReturnNever()
	{
		var mockInteractions = new MockInteractions();
		IMockInteractions interactions = mockInteractions;
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		var result = accessed.PropertySetter("foo.bar", With.Any<string>());

		await That(result).Never();
	}
}
