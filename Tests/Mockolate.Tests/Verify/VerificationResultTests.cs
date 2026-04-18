using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public sealed partial class VerificationResultTests
{
	[Fact]
	public async Task CustomVerificationResult_ShouldKeepExpectation()
	{
		VerificationResult<int> sut = new(1, new MockInteractions(), _ => false, "foo");

		string result = ((IVerificationResult)sut).Expectation;

		await That(result).IsEqualTo("foo");
	}

	[Fact]
	public async Task VerificationResult_Got_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result
			= sut.Mock.Verify.TotalDispensed.Got();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property TotalDispensed");
	}

	[Fact]
	public async Task VerificationResult_GotIndexer_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result
			= sut.Mock.Verify[It.IsAny<string>()].Got();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("got indexer [It.IsAny<string>()]");
	}

	[Fact]
	public async Task VerificationResult_GotIndexerWithMultipleParameters_ShouldHaveExpectedValue()
	{
		IIndexerVerificationService sut = IIndexerVerificationService.CreateMock();

		VerificationResult<Mock.IMockVerifyForVerificationResultTests_IIndexerVerificationService> result = sut.Mock.Verify[It.IsAny<string>(), null].Got();

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("got indexer [It.IsAny<string>(), null]");
	}

	[Fact]
	public async Task VerificationResult_Invoked_AnyParameters_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result
			= sut.Mock.Verify.Dispense(Match.AnyParameters());

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("invoked method Dispense(Match.AnyParameters())");
	}

	[Fact]
	public async Task VerificationResult_Invoked_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result
			= sut.Mock.Verify.Dispense(It.IsAny<string>(), It.IsAny<int>());

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("invoked method Dispense(It.IsAny<string>(), It.IsAny<int>())");
	}

	[Fact]
	public async Task VerificationResult_MockInteractions_HasAllInteractions()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		sut.Dispense("Dark", 2);
		_ = sut.TotalDispensed;
		sut["Dark"] = 5;
		sut.TotalDispensed = 10;
		_ = sut["Milk"];

		VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result
			= sut.Mock.Verify.TotalDispensed.Got();

		await That(((IVerificationResult)result).MockInteractions.Count).IsEqualTo(5);
	}

	[Fact]
	public async Task VerificationResult_Set_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result
			= sut.Mock.Verify.TotalDispensed.Set(It.Is(5));

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set property TotalDispensed to 5");
	}

	[Fact]
	public async Task VerificationResult_SetIndexer_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result
			= sut.Mock.Verify[It.IsAny<string>()].Set(It.Is(5));

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set indexer [It.IsAny<string>()] to 5");
	}

	[Fact]
	public async Task VerificationResult_SetIndexerWithMultipleParameters_ShouldHaveExpectedValue()
	{
		IIndexerVerificationService sut = IIndexerVerificationService.CreateMock();

		VerificationResult<Mock.IMockVerifyForVerificationResultTests_IIndexerVerificationService> result = sut.Mock.Verify[It.Is("foo"), null].Set(It.Is<int?>(5));

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set indexer [\"foo\", null] to 5");
	}

	[Fact]
	public async Task VerificationResult_SetIndexerWithMultipleParametersToNull_ShouldHaveExpectedValue()
	{
		IIndexerVerificationService sut = IIndexerVerificationService.CreateMock();

		VerificationResult<Mock.IMockVerifyForVerificationResultTests_IIndexerVerificationService> result = sut.Mock.Verify[It.Is("foo"), null].Set(null);

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set indexer [\"foo\", null] to null");
	}

	[Fact]
	public async Task VerificationResult_SubscribedTo_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result
			= sut.Mock.Verify.ChocolateDispensed.Subscribed();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("subscribed to event ChocolateDispensed");
	}

	[Fact]
	public async Task VerificationResult_UnsubscribedFrom_ShouldHaveExpectedValue()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();

		VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result
			= sut.Mock.Verify.ChocolateDispensed.Unsubscribed();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("unsubscribed from event ChocolateDispensed");
	}

	internal interface IIndexerVerificationService
	{
		int? this[string p1, int? p2] { get; set; }
	}
}
