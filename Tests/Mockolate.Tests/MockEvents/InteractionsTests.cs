using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockEvents;

public sealed class InteractionsTests
{
	[Fact]
	public async Task EventSubscription_SetIndexTwice_ShouldRemainUnchanged()
	{
		EventSubscription interaction = new("SomeEvent", this, Helper.GetMethodInfo())
		{
			Index = 1,
		};

		interaction.Index = 2;

		await That(interaction.Index).IsEqualTo(1);
	}

	[Fact]
	public async Task EventSubscription_ToString_ShouldReturnExpectedValue()
	{
		EventSubscription interaction = new("SomeEvent", this, Helper.GetMethodInfo())
		{
			Index = 3,
		};
		string expectedValue = "[3] subscribe to event SomeEvent";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task EventUnsubscription_SetIndexTwice_ShouldRemainUnchanged()
	{
		EventUnsubscription interaction = new("SomeEvent", this, Helper.GetMethodInfo())
		{
			Index = 1,
		};

		interaction.Index = 2;

		await That(interaction.Index).IsEqualTo(1);
	}

	[Fact]
	public async Task EventUnsubscription_ToString_ShouldReturnExpectedValue()
	{
		EventUnsubscription interaction = new("SomeEvent", this, Helper.GetMethodInfo());
		interaction.Index = 3;
		string expectedValue = "[3] unsubscribe from event SomeEvent";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task Subscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registration = ((IMock)sut).MockRegistry;
		registration.AddEvent("foo.bar", this, Helper.GetMethodInfo());

		VerificationResult<IChocolateDispenser> result = registration.SubscribedTo(sut, "baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task Subscribed_WhenNameMatches_ShouldReturnOnce()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registration = ((IMock)sut).MockRegistry;
		registration.AddEvent("foo.bar", this, Helper.GetMethodInfo());

		VerificationResult<IChocolateDispenser> result = registration.SubscribedTo(sut, "foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task Subscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registration = ((IMock)sut).MockRegistry;

		VerificationResult<IChocolateDispenser> result = registration.SubscribedTo(sut, "baz.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("subscribed to event bar");
	}

	[Fact]
	public async Task Unsubscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registration = ((IMock)sut).MockRegistry;
		registration.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());

		VerificationResult<IChocolateDispenser> result = registration.UnsubscribedFrom(sut, "baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task Unsubscribed_WhenNameMatches_ShouldReturnOnce()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registration = ((IMock)sut).MockRegistry;
		registration.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());

		VerificationResult<IChocolateDispenser> result = registration.UnsubscribedFrom(sut, "foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task Unsubscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registration = ((IMock)sut).MockRegistry;

		VerificationResult<IChocolateDispenser> result = registration.UnsubscribedFrom(sut, "baz.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("unsubscribed from event bar");
	}
}
