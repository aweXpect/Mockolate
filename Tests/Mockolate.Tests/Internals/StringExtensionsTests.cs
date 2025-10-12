using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Internals;

public sealed class StringExtensionsTests
{
	[Fact]
	public async Task SubstringAfterLast_WhenNameContainsNoDot_ShouldIncludeFullName()
	{
		MockInteractions interactions = new();
		MockVerify<int, Mock<int>> verify = new(interactions, new MyMock<int>(1));
		IMockGot<MockVerify<int, Mock<int>>> mockGot = new MockGot<int, Mock<int>>(verify);

		var result = mockGot.Property("SomeNameWithoutADot");

		result.Never();
		await That(result.Expectation).IsEqualTo("got property SomeNameWithoutADot");
	}

	[Fact]
	public async Task SubstringAfterLast_WhenNameStartsWithDot_ShouldOmitDot()
	{
		MockInteractions interactions = new();
		MockVerify<int, Mock<int>> verify = new(interactions, new MyMock<int>(1));
		IMockGot<MockVerify<int, Mock<int>>> mockGot = new MockGot<int, Mock<int>>(verify);

		var result = mockGot.Property(".bar");

		result.Never();
		await That(result.Expectation).IsEqualTo("got property bar");
	}
}
