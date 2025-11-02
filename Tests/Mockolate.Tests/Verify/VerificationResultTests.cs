using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public class VerificationResultTests
{
	[Fact]
	public async Task Expectation_EventSubscription_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		VerificationResult<MockVerify<IMyService, Mock<IMyService>>> result =
			sut.Verify.SubscribedTo.MyEvent();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("subscribed to event MyEvent");
	}

	[Fact]
	public async Task Expectation_EventUnsubscription_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		var result = sut.Verify.UnsubscribedFrom.MyEvent();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("unsubscribed from event MyEvent");
	}

	[Fact]
	public async Task Expectation_Method_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		var result = sut.Verify.Invoked.DoSomethingAndReturn(With.Any<int>());

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("invoked method DoSomethingAndReturn(With.Any<int>())");
	}

	[Fact]
	public async Task Expectation_PropertyGetter_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		var result = sut.Verify.Got.SomeFlag();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property SomeFlag");
	}

	[Fact]
	public async Task Expectation_PropertySetter_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		var result = sut.Verify.Set.SomeFlag(With.Any<bool>());

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set property SomeFlag to value With.Any<bool>()");
	}
}
