using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public class VerificationResultTests
{
	[Fact]
	public async Task Expectation_EventSubscription_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		VerificationResult<MockVerify<IMyService, Mock<IMyService>>> result =
			sut.Verify.SubscribedTo.SomethingHappened();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("subscribed to event SomethingHappened");
	}

	[Fact]
	public async Task Expectation_EventUnsubscription_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		var result = sut.Verify.UnsubscribedFrom.SomethingHappened();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("unsubscribed from event SomethingHappened");
	}

	[Fact]
	public async Task Expectation_Method_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		var result = sut.Verify.Invoked.DoSomething(With.Any<int?>(), "foo");

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("invoked method DoSomething(With.Any<int?>(), \"foo\")");
	}

	[Fact]
	public async Task Expectation_PropertyGetter_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		var result = sut.Verify.Got.MyProperty();

		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property MyProperty");
	}

	[Fact]
	public async Task Expectation_PropertySetter_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		var result = sut.Verify.Set.MyProperty(With.Any<int>());

		await That(((IVerificationResult)result).Expectation)
			.IsEqualTo("set property MyProperty to value With.Any<int>()");
	}

	public interface IMyService
	{
		int MyProperty { get; set; }
		event EventHandler? SomethingHappened;
		void DoSomething(int? value, string otherValue);
	}
}
