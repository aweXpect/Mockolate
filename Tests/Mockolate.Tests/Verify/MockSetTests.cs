using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockSetTests
{
	[Fact]
	public void WhenNameAndValueMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSet<Mock<int>> mockSet = new MockSet<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		VerificationResult<Mock<int>> result = mockSet.Property("foo.bar", With.Any<int>());

		result.Once();
	}

	[Fact]
	public void WhenOnlyNameMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSet<Mock<int>> mockSet = new MockSet<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		VerificationResult<Mock<int>> result = mockSet.Property("foo.bar", With.Any<string>());

		result.Never();
	}

	[Fact]
	public void WhenOnlyValueMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSet<Mock<int>> mockSet = new MockSet<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		VerificationResult<Mock<int>> result = mockSet.Property("baz.bar", With.Any<int>());

		result.Never();
	}

	[Fact]
	public async Task WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSet<Mock<int>> mockSet = new MockSet<int, Mock<int>>(verify);

		VerificationResult<Mock<int>> result = mockSet.Property("foo.bar", With.Any<int>());

		result.Never();
		await That(result.Expectation).IsEqualTo("set property bar to value With.Any<int>()");
	}
}
