using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public class VerificationResultTests
{
	[Fact]
	public async Task VerificationResult_Got_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IMockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.Got.TotalDispensed();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property TotalDispensed");
	}

	[Fact]
	public async Task VerificationResult_GotIndexer_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IMockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.GotIndexer(WithAny<string>());

		await That(((IVerificationResult)result).Expectation).IsEqualTo("got indexer WithAny<string>()");
	}

	[Fact]
	public async Task VerificationResult_Invoked_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IMockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.Invoked.Dispense(WithAny<string>(), WithAny<int>());

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("invoked method Dispense(WithAny<string>(), WithAny<int>())");
	}

	[Fact]
	public async Task VerificationResult_Set_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IMockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.Set.TotalDispensed(With(5));

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set property TotalDispensed to value 5");
	}

	[Fact]
	public async Task VerificationResult_SetIndexer_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IMockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.SetIndexer(WithAny<string>(), With(5));

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set indexer WithAny<string>() to value 5");
	}

	[Fact]
	public async Task VerificationResult_SubscribedTo_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IMockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.SubscribedTo.ChocolateDispensed();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("subscribed to event ChocolateDispensed");
	}

	[Fact]
	public async Task VerificationResult_UnsubscribedFrom_ShouldHaveExpectedValue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IMockVerify<IChocolateDispenser, Mock<IChocolateDispenser>>> result
			= sut.Verify.UnsubscribedFrom.ChocolateDispensed();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("unsubscribed from event ChocolateDispensed");
	}
}
