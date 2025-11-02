using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public class VerificationResultTests
{
	[Fact]
	public async Task VerificationResult_Got_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<MockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.Got.TotalDispensed();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property TotalDispensed");
	}

	[Fact]
	public async Task VerificationResult_GotIndexer_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<MockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.GotIndexer(With.Any<string>());

		await That(((IVerificationResult)result).Expectation).IsEqualTo("got indexer With.Any<string>()");
	}

	[Fact]
	public async Task VerificationResult_Invoked_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<MockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.Invoked.Dispense(With.Any<string>(), With.Any<int>());

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("invoked method Dispense(With.Any<string>(), With.Any<int>())");
	}

	[Fact]
	public async Task VerificationResult_Set_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<MockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.Set.TotalDispensed(5);

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set property TotalDispensed to value 5");
	}

	[Fact]
	public async Task VerificationResult_SetIndexer_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<MockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.SetIndexer(With.Any<string>(), 5);

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set indexer With.Any<string>() to value 5");
	}

	[Fact]
	public async Task VerificationResult_SubscribedTo_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<MockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.SubscribedTo.ChocolateDispensed();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("subscribed to event ChocolateDispensed");
	}

	[Fact]
	public async Task VerificationResult_UnsubscribedFrom_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<MockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.UnsubscribedFrom.ChocolateDispensed();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("unsubscribed from event ChocolateDispensed");
	}
}
