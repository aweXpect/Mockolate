using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public class VerificationResultTests
{
	[Fact]
	public async Task VerificationResult_Got_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IChocolateDispenser> result
			= sut.VerifyMock.Got.TotalDispensed();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property TotalDispensed");
	}

	[Fact]
	public async Task VerificationResult_GotIndexer_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IChocolateDispenser> result
			= sut.VerifyMock.GotIndexer(Any<string>());

		await That(((IVerificationResult)result).Expectation).IsEqualTo("got indexer Any<string>()");
	}

	[Fact]
	public async Task VerificationResult_Invoked_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IChocolateDispenser> result
			= sut.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>());

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("invoked method Dispense(Any<string>(), Any<int>())");
	}

	[Fact]
	public async Task VerificationResult_Set_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IChocolateDispenser> result
			= sut.VerifyMock.Set.TotalDispensed(With(5));

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set property TotalDispensed to value 5");
	}

	[Fact]
	public async Task VerificationResult_SetIndexer_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IChocolateDispenser> result
			= sut.VerifyMock.SetIndexer(Any<string>(), With(5));

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set indexer Any<string>() to value 5");
	}

	[Fact]
	public async Task VerificationResult_SubscribedTo_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IChocolateDispenser> result
			= sut.VerifyMock.SubscribedTo.ChocolateDispensed();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("subscribed to event ChocolateDispensed");
	}

	[Fact]
	public async Task VerificationResult_UnsubscribedFrom_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IChocolateDispenser> result
			= sut.VerifyMock.UnsubscribedFrom.ChocolateDispensed();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("unsubscribed from event ChocolateDispensed");
	}
}
