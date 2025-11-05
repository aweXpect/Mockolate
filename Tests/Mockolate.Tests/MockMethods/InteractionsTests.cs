using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockMethods;
/* TODO Re-enable
public sealed class InteractionsTests
{
	[Fact]
	public async Task Method_WhenNameAndValueMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockInvoked<IMockVerify<int, Mock<int>>> invoked = new MockInvoked<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [4,]));

		VerificationResult<IMockVerify<int, Mock<int>>> result = invoked.Method("foo.bar", With.Any<int>());

		await That(result).Once();
	}

	[Fact]
	public async Task Method_WhenOnlyNameMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockInvoked<IMockVerify<int, Mock<int>>> invoked = new MockInvoked<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [4,]));

		VerificationResult<IMockVerify<int, Mock<int>>> result = invoked.Method("foo.bar", With.Any<string>());

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WhenOnlyValueMatches_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockInvoked<IMockVerify<int, Mock<int>>> invoked = new MockInvoked<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [4,]));

		VerificationResult<IMockVerify<int, Mock<int>>> result = invoked.Method("baz.bar", With.Any<int>());

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockInvoked<IMockVerify<int, Mock<int>>> invoked = new MockInvoked<int, Mock<int>>(verify);

		VerificationResult<IMockVerify<int, Mock<int>>> result = invoked.Method("foo.bar", With.Any<int>());

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("invoked method bar(With.Any<int>())");
	}

	[Fact]
	public async Task MethodInvocation_ToString_ShouldReturnExpectedValue()
	{
		MethodInvocation interaction = new(3, "SomeMethod", [1, null, TimeSpan.FromSeconds(90),]);
		string expectedValue = "[3] invoke method SomeMethod(1, null, 00:01:30)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	public sealed class ProtectedTests
	{
		[Fact]
		public async Task PropertySetter_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			MockInvoked<int, Mock<int>> inner = new(verify);
			IMockInvoked<IMockVerify<int, Mock<int>>> invoked = inner;
			IMockInvoked<IMockVerify<int, Mock<int>>> @protected = new ProtectedMockInvoked<int, Mock<int>>(inner);
			interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [1,]));
			interactions.RegisterInteraction(new MethodInvocation(1, "foo.bar", [2,]));

			VerificationResult<IMockVerify<int, Mock<int>>> result1 = invoked.Method("foo.bar", With.Any<int>());
			VerificationResult<IMockVerify<int, Mock<int>>> result2 = @protected.Method("foo.bar", With.Any<int>());

			await That(result1).Exactly(2);
			await That(result2).Exactly(2);
		}
	}
}
*/
