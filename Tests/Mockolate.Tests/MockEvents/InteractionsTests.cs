using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockEvents;

public sealed partial class InteractionsTests
{
	[Fact]
	public async Task EventSubscription_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		EventSubscription interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new EventSubscription("global::Mockolate.InteractionsTests.SomeEvent", this, Helper.GetMethodInfo()));
		string expectedValue = "subscribe to event SomeEvent";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task EventUnsubscription_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		EventUnsubscription interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new EventUnsubscription("global::Mockolate.InteractionsTests.SomeEvent", this, Helper.GetMethodInfo()));
		string expectedValue = "unsubscribe from event SomeEvent";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task Subscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;
		registry.AddEvent("foo.bar", this, Helper.GetMethodInfo());

		VerificationResult<IChocolateDispenser> result = registry.SubscribedTo(sut, "baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task Subscribed_WhenNameMatches_ShouldReturnOnce()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;
		registry.AddEvent("foo.bar", this, Helper.GetMethodInfo());

		VerificationResult<IChocolateDispenser> result = registry.SubscribedTo(sut, "foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task Subscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;

		VerificationResult<IChocolateDispenser> result = registry.SubscribedTo(sut, "baz.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("subscribed to event bar");
	}

	[Fact]
	public async Task Unsubscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;
		registry.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());

		VerificationResult<IChocolateDispenser> result = registry.UnsubscribedFrom(sut, "baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task Unsubscribed_WhenNameMatches_ShouldReturnOnce()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;
		registry.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());

		VerificationResult<IChocolateDispenser> result = registry.UnsubscribedFrom(sut, "foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task Unsubscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;

		VerificationResult<IChocolateDispenser> result = registry.UnsubscribedFrom(sut, "baz.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("unsubscribed from event bar");
	}
}
