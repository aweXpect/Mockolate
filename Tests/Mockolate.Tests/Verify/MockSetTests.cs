using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockSetTests
{
	[Fact]
	public async Task WhenNameAndValueMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSet<MockVerify<int, Mock<int>>> mockSet = new MockSet<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		VerificationResult<MockVerify<int, Mock<int>>> result = mockSet.Property("foo.bar", With.Any<int>());

		await That(result).Once();
	}

	[Fact]
	public async Task WhenOnlyNameMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSet<MockVerify<int, Mock<int>>> mockSet = new MockSet<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		VerificationResult<MockVerify<int, Mock<int>>> result = mockSet.Property("foo.bar", With.Any<string>());

		await That(result).Never();
	}

	[Fact]
	public async Task WhenOnlyValueMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSet<MockVerify<int, Mock<int>>> mockSet = new MockSet<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new PropertySetterAccess(0, "foo.bar", 4));

		VerificationResult<MockVerify<int, Mock<int>>> result = mockSet.Property("baz.bar", With.Any<int>());

		await That(result).Never();
	}

	[Fact]
	public async Task WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSet<MockVerify<int, Mock<int>>> mockSet = new MockSet<int, Mock<int>>(verify);

		VerificationResult<MockVerify<int, Mock<int>>> result = mockSet.Property("foo.bar", With.Any<int>());

		await That(result).Never();
		await That((((IVerificationResult)result).Expectation)).IsEqualTo("set property bar to value With.Any<int>()");
	}
}
