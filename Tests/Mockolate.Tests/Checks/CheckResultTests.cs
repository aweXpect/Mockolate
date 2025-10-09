using Mockolate.Checks;

namespace Mockolate.Tests.Checks;

public class CheckResultTests
{
	[Fact]
	public async Task Expectation_EventSubscription_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		CheckResult<Mock<IMyService>> check = sut.Event.SomethingHappened.Subscribed();

		await That(check.Expectation).IsEqualTo("subscribed to event SomethingHappened");
	}

	[Fact]
	public async Task Expectation_EventUnsubscription_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		CheckResult<Mock<IMyService>> check = sut.Event.SomethingHappened.Unsubscribed();

		await That(check.Expectation).IsEqualTo("unsubscribed from event SomethingHappened");
	}

	[Fact]
	public async Task Expectation_Method_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		CheckResult<Mock<IMyService>> check = sut.Invoked.DoSomething(With.Any<int?>(), "foo");

		await That(check.Expectation).IsEqualTo("invoked method DoSomething(With.Any<int?>(), \"foo\")");
	}

	[Fact]
	public async Task Expectation_PropertyGetter_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		CheckResult<Mock<IMyService>> check = sut.Accessed.MyProperty.Getter();

		await That(check.Expectation).IsEqualTo("accessed getter of property MyProperty");
	}

	[Fact]
	public async Task Expectation_PropertySetter_ShouldHaveExpectedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		CheckResult<Mock<IMyService>> check = sut.Accessed.MyProperty.Setter(With.Any<int>());

		await That(check.Expectation).IsEqualTo("accessed setter of property MyProperty with value With.Any<int>()");
	}

	public interface IMyService
	{
		int MyProperty { get; set; }
		event EventHandler? SomethingHappened;
		void DoSomething(int? value, string otherValue);
	}
}
