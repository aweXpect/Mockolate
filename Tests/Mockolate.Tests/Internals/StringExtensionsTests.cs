using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Internals;

public sealed class StringExtensionsTests
{
	/* TODO
	[Fact]
	public async Task SubstringAfterLast_WhenNameContainsNoDot_ShouldIncludeFullName()
	{
		MockInteractions interactions = new();
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(interactions, new MyMock<int>(1));

		CheckResult<Mock<int>> result = accessed.PropertyGetter("SomeNameWithoutADot");

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("accessed getter of property SomeNameWithoutADot");
	}

	[Fact]
	public async Task SubstringAfterLast_WhenNameStartsWithDot_ShouldOmitDot()
	{
		MockInteractions interactions = new();
		IMockAccessed<Mock<int>> accessed = new MockAccessed<int, Mock<int>>(interactions, new MyMock<int>(1));

		CheckResult<Mock<int>> result = accessed.PropertyGetter(".bar");

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("accessed getter of property bar");
	}
	*/
}
