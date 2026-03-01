using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockEvents;

public sealed class InteractionsTests
{
	[Test]
	public async Task EventSubscription_ToString_ShouldReturnExpectedValue()
	{
		EventSubscription interaction = new(3, "SomeEvent", this, Helper.GetMethodInfo());
		string expectedValue = "[3] subscribe to event SomeEvent";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Test]
	public async Task EventUnsubscription_ToString_ShouldReturnExpectedValue()
	{
		EventUnsubscription interaction = new(3, "SomeEvent", this, Helper.GetMethodInfo());
		string expectedValue = "[3] unsubscribe from event SomeEvent";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Test]
	public async Task Subscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;
		registration.AddEvent("foo.bar", this, Helper.GetMethodInfo());

		VerificationResult<IChocolateDispenser> result = registration.SubscribedTo(mock, "baz.bar");

		await That(result).Never();
	}

	[Test]
	public async Task Subscribed_WhenNameMatches_ShouldReturnOnce()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;
		registration.AddEvent("foo.bar", this, Helper.GetMethodInfo());

		VerificationResult<IChocolateDispenser> result = registration.SubscribedTo(mock, "foo.bar");

		await That(result).Once();
	}

	[Test]
	public async Task Subscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

		VerificationResult<IChocolateDispenser> result = registration.SubscribedTo(mock, "baz.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("subscribed to event bar");
	}

	[Test]
	public async Task Unsubscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;
		registration.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());

		VerificationResult<IChocolateDispenser> result = registration.UnsubscribedFrom(mock, "baz.bar");

		await That(result).Never();
	}

	[Test]
	public async Task Unsubscribed_WhenNameMatches_ShouldReturnOnce()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;
		registration.RemoveEvent("foo.bar", this, Helper.GetMethodInfo());

		VerificationResult<IChocolateDispenser> result = registration.UnsubscribedFrom(mock, "foo.bar");

		await That(result).Once();
	}

	[Test]
	public async Task Unsubscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

		VerificationResult<IChocolateDispenser> result = registration.UnsubscribedFrom(mock, "baz.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("unsubscribed from event bar");
	}
}
