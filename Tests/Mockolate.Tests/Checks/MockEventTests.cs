using System.Reflection;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Checks;

public sealed partial class MockEventTests
{
	/* TODO
	[Fact]
	public async Task Subscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		IMockEvent<Mock<int>> @event = new MockEvent<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, GetMethodInfo()));

		CheckResult<Mock<int>> result = @event.Subscribed("baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task Subscribed_WhenNameMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		IMockEvent<Mock<int>> @event = new MockEvent<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, GetMethodInfo()));

		CheckResult<Mock<int>> result = @event.Subscribed("foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task Subscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions interactions = new();
		IMockEvent<Mock<int>> @event = new MockEvent<int, Mock<int>>(interactions, new MyMock<int>(1));

		CheckResult<Mock<int>> result = @event.Subscribed("foo.bar");

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("subscribed to event bar");
	}

	[Fact]
	public async Task Unsubscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		IMockEvent<Mock<int>> @event = new MockEvent<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, GetMethodInfo()));

		CheckResult<Mock<int>> result = @event.Unsubscribed("baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task Unsubscribed_WhenNameMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		IMockEvent<Mock<int>> @event = new MockEvent<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, GetMethodInfo()));

		CheckResult<Mock<int>> result = @event.Unsubscribed("foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task Unsubscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions interactions = new();
		IMockEvent<Mock<int>> @event = new MockEvent<int, Mock<int>>(interactions, new MyMock<int>(1));

		CheckResult<Mock<int>> result = @event.Unsubscribed("foo.bar");

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("unsubscribed from event bar");
	}

	private static MethodInfo GetMethodInfo()
		=> typeof(MockEventTests).GetMethod(nameof(GetMethodInfo), BindingFlags.Static)!;
	*/
}
