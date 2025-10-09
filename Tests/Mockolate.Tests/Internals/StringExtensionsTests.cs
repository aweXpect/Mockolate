using Mockolate.Checks;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Internals;

public sealed class StringExtensionsTests
{
	[Fact]
	public async Task SubstringAfterLast_WhenNameStartsWithDot_ShouldOmitDot()
	{
		var interactions = new MockInteractions();
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(interactions, new MyMock<int>(1));

		var result = accessed.PropertyGetter(".bar");

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("accessed getter of property bar");
	}

	[Fact]
	public async Task SubstringAfterLast_WhenNameContainsNoDot_ShouldIncludeFullName()
	{
		var interactions = new MockInteractions();
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(interactions, new MyMock<int>(1));

		var result = accessed.PropertyGetter("SomeNameWithoutADot");

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("accessed getter of property SomeNameWithoutADot");
	}
}
