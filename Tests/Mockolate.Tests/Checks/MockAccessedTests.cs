using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Checks;

public sealed partial class MockAccessedTests
{/* TODO
	[Fact]
	public async Task PropertyGetter_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new PropertyGetterAccess(0, "foo.bar"));

		CheckResult<Mock<int>> result = accessed.PropertyGetter("baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task PropertyGetter_WhenNameMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new PropertyGetterAccess(0, "foo.bar"));

		CheckResult<Mock<int>> result = accessed.PropertyGetter("foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task PropertyGetter_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions interactions = new();
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(interactions, new MyMock<int>(1));

		CheckResult<Mock<int>> result = accessed.PropertyGetter("foo.bar");

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("accessed getter of property bar");
	}

	[Fact]
	public async Task PropertySetter_WhenNameAndValueMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		CheckResult<Mock<int>> result = accessed.PropertySetter("foo.bar", With.Any<int>());

		await That(result).Once();
	}

	[Fact]
	public async Task PropertySetter_WhenOnlyNameMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		CheckResult<Mock<int>> result = accessed.PropertySetter("foo.bar", With.Any<string>());

		await That(result).Never();
	}

	[Fact]
	public async Task PropertySetter_WhenOnlyValueMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		CheckResult<Mock<int>> result = accessed.PropertySetter("baz.bar", With.Any<int>());

		await That(result).Never();
	}

	[Fact]
	public async Task PropertySetter_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions interactions = new();
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(interactions, new MyMock<int>(1));

		CheckResult<Mock<int>> result = accessed.PropertySetter("foo.bar", With.Any<int>());

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("accessed setter of property bar with value With.Any<int>()");
	}
	*/
}
