using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockMethods;

public sealed partial class MockInvokedTests
{
	[Fact]
	public async Task Method_WhenNameAndValueMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockInvoked<MockVerify<int, Mock<int>>> invoked = new MockInvoked<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [4,]));

		VerificationResult<MockVerify<int, Mock<int>>> result = invoked.Method("foo.bar", With.Any<int>());

		await That(result).Once();
	}

	[Fact]
	public async Task Method_WhenOnlyNameMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockInvoked<MockVerify<int, Mock<int>>> invoked = new MockInvoked<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [4,]));

		VerificationResult<MockVerify<int, Mock<int>>> result = invoked.Method("foo.bar", With.Any<string>());

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WhenOnlyValueMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockInvoked<MockVerify<int, Mock<int>>> invoked = new MockInvoked<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [4,]));

		VerificationResult<MockVerify<int, Mock<int>>> result = invoked.Method("baz.bar", With.Any<int>());

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockInvoked<MockVerify<int, Mock<int>>> invoked = new MockInvoked<int, Mock<int>>(verify);

		VerificationResult<MockVerify<int, Mock<int>>> result = invoked.Method("foo.bar", With.Any<int>());

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("invoked method bar(With.Any<int>())");
	}
}
