using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public sealed partial class VerificationResultTests
{
	[Fact]
	public async Task CustomVerificationResult_ShouldKeepExpectation()
	{
		IMockInteractions interactions = new MockInteractions();
		VerificationResult<int> sut = new(1, interactions, _ => false, "foo");

		string result = ((IVerificationResult)sut).Expectation;

		await That(result).IsEqualTo("foo");
	}

	[Fact]
	public async Task CustomVerificationResult_WithFuncExpectation_ShouldKeepExpectation()
	{
		IMockInteractions interactions = new MockInteractions();
		VerificationResult<int> sut = new(1, interactions, _ => false, () => "foo");

		string result = ((IVerificationResult)sut).Expectation;

		await That(result).IsEqualTo("foo");
	}

	[Fact]
	public async Task CustomVerificationResult_WithIMockInteractions_ShouldExposeInteractions()
	{
		IMockInteractions interactions = new MockInteractions();
		VerificationResult<int> sut = new(1, interactions, _ => false, "foo");

		IMockInteractions exposed = ((IVerificationResult)sut).Interactions;

		await That(exposed).IsSameAs(interactions);
	}

#pragma warning disable CS0618 // forwarding-shim coverage
	[Fact]
	public async Task CustomVerificationResult_WithMockInteractionsObsoleteOverload_ShouldForward()
	{
		MockInteractions interactions = new();
		VerificationResult<int> sut = new(1, interactions, _ => false, "foo");

		string result = ((IVerificationResult)sut).Expectation;

		await That(result).IsEqualTo("foo");
	}

	[Fact]
	public async Task CustomVerificationResult_WithMockInteractionsObsoleteFuncOverload_ShouldForward()
	{
		MockInteractions interactions = new();
		VerificationResult<int> sut = new(1, interactions, _ => false, () => "foo");

		string result = ((IVerificationResult)sut).Expectation;

		await That(result).IsEqualTo("foo");
	}
#pragma warning restore CS0618

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

		await That(((IVerificationResult)result).Interactions.Count).IsEqualTo(5);
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

	[Fact]
	public async Task AnyParameters_OnOverloadedMethod_FastPath_ShouldOnlyCountTheTargetedOverload()
	{
		IOverloadedMethodService sut = IOverloadedMethodService.CreateMock();

		sut.DoSomething(1);
		sut.DoSomething(2);
		sut.DoSomething(3, true);

		await That(sut.Mock.Verify.DoSomething(0).AnyParameters()).Exactly(2);
		await That(sut.Mock.Verify.DoSomething(0, false).AnyParameters()).Once();
	}

	[Fact]
	public async Task AnyParameters_OnOverloadedGenericMethod_LegacyPath_ShouldOnlyCountTheTargetedOverload()
	{
		IOverloadedMethodService sut = IOverloadedMethodService.CreateMock();

		sut.GetInstance<int>();
		sut.GetInstance<int>();
		sut.GetInstance<int>("key");

		await That(sut.Mock.Verify.GetInstance<int>().AnyParameters()).Exactly(2);
		await That(sut.Mock.Verify.GetInstance<int>("ignored").AnyParameters()).Once();
	}

	internal interface IIndexerVerificationService
	{
		int? this[string p1, int? p2] { get; set; }
	}

	internal interface IOverloadedMethodService
	{
		void DoSomething(int value);
		void DoSomething(int value, bool flag);

		TService GetInstance<TService>();
		TService GetInstance<TService>(string key);
	}
}
