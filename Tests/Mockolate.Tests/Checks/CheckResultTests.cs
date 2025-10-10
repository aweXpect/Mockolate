using Mockolate.Checks;

namespace Mockolate.Tests.Checks;

public class CheckResultTests
{
	[Fact]
	public async Task Expectation_EventSubscription_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		CheckResult<Mock<IMyService>> check = sut.Verify.Event.SomethingHappened.Subscribed();

		await That(check.Expectation).IsEqualTo("subscribed to event SomethingHappened");
	}

	[Fact]
	public async Task Expectation_EventUnsubscription_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		CheckResult<Mock<IMyService>> check = sut.Verify.Event.SomethingHappened.Unsubscribed();

		await That(check.Expectation).IsEqualTo("unsubscribed from event SomethingHappened");
	}

	[Fact]
	public async Task Expectation_Method_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		CheckResult<Mock<IMyService>> check = sut.Verify.Invoked.DoSomething(With.Any<int?>(), "foo");

		await That(check.Expectation).IsEqualTo("invoked method DoSomething(With.Any<int?>(), \"foo\")");
	}

	[Fact]
	public async Task Expectation_PropertyGetter_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		CheckResult<Mock<IMyService>> check = sut.Verify.Got.MyProperty();

		await That(check.Expectation).IsEqualTo("got property MyProperty");
	}

	[Fact]
	public async Task Expectation_PropertySetter_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		CheckResult<Mock<IMyService>> check = sut.Verify.Set.MyProperty(With.Any<int>());

		await That(check.Expectation).IsEqualTo("set property MyProperty to value With.Any<int>()");
	}

	public interface IMyService
	{
		int MyProperty { get; set; }
		event EventHandler? SomethingHappened;
		void DoSomething(int? value, string otherValue);
	}
}
