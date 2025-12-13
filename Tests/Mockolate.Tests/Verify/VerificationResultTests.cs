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
			= sut.VerifyMock.GotIndexer(It.IsAny<string>());

		await That(((IVerificationResult)result).Expectation).IsEqualTo("got indexer [It.IsAny<string>()]");
	}

	[Fact]
	public async Task VerificationResult_GotIndexerWithMultipleParameters_ShouldHaveExpectedValue()
	{
		IIndexerVerificationService sut = Mock.Create<IIndexerVerificationService>();

		VerificationResult<IIndexerVerificationService> result
			= sut.VerifyMock.GotIndexer(It.IsAny<string>(), null);

		await That(((IVerificationResult)result).Expectation).IsEqualTo("got indexer [It.IsAny<string>(), It.IsNull<int?>()]");
	}

	[Fact]
	public async Task VerificationResult_Invoked_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IChocolateDispenser> result
			= sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>());

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("invoked method Dispense(It.IsAny<string>(), It.IsAny<int>())");
	}

	[Fact]
	public async Task VerificationResult_MockInteractions_HasAllInteractions()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		sut.Dispense("Dark", 2);
		_ = sut.TotalDispensed;
		sut["Dark"] = 5;
		sut.TotalDispensed = 10;
		_ = sut["Milk"];

		VerificationResult<IChocolateDispenser> result
			= sut.VerifyMock.Got.TotalDispensed();

		await That(((IVerificationResult)result).MockInteractions.Count).IsEqualTo(5);
	}

	[Fact]
	public async Task VerificationResult_Set_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IChocolateDispenser> result
			= sut.VerifyMock.Set.TotalDispensed(It.Is(5));

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set property TotalDispensed to value 5");
	}

	[Fact]
	public async Task VerificationResult_SetIndexer_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		VerificationResult<IChocolateDispenser> result
			= sut.VerifyMock.SetIndexer(It.IsAny<string>(), It.Is(5));

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set indexer [It.IsAny<string>()] to value 5");
	}

	[Fact]
	public async Task VerificationResult_SetIndexerWithMultipleParameters_ShouldHaveExpectedValue()
	{
		IIndexerVerificationService sut = Mock.Create<IIndexerVerificationService>();

		VerificationResult<IIndexerVerificationService> result
			= sut.VerifyMock.SetIndexer(It.Is("foo"), null, It.Is(5));

		await That(((IVerificationResult)result).Expectation).IsEqualTo("set indexer [\"foo\", It.IsNull<int?>()] to value 5");
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
	
	internal interface IIndexerVerificationService
	{
		int this[string p1, int? p2] { get; set; }
	}
}
