using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockMethods;
public sealed class InteractionsTests
{
	[Fact]
	public async Task Method_WhenNameAndValueMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockInvoked<IMockVerify<int, Mock<int>>> invoked = verify;
		interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [4,]));

		VerificationResult<IMockVerify<int, Mock<int>>> result = invoked.Method("foo.bar", WithAny<int>());

		await That(result).Once();
	}

	[Fact]
	public async Task Method_WhenOnlyNameMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockInvoked<IMockVerify<int, Mock<int>>> invoked = verify;
		interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [4,]));

		VerificationResult<IMockVerify<int, Mock<int>>> result = invoked.Method("foo.bar", WithAny<string>());

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WhenOnlyValueMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockInvoked<IMockVerify<int, Mock<int>>> invoked = verify;
		interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [4,]));

		VerificationResult<IMockVerify<int, Mock<int>>> result = invoked.Method("baz.bar", WithAny<int>());

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockInvoked<IMockVerify<int, Mock<int>>> invoked = verify;

		VerificationResult<IMockVerify<int, Mock<int>>> result = invoked.Method("foo.bar", WithAny<int>());

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("invoked method bar(WithAny<int>())");
	}

	[Fact]
	public async Task MethodInvocation_ToString_ShouldReturnExpectedValue()
	{
		MethodInvocation interaction = new(3, "SomeMethod", [1, null, TimeSpan.FromSeconds(90),]);
		string expectedValue = "[3] invoke method SomeMethod(1, null, 00:01:30)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
