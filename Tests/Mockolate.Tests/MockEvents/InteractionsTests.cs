using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockEvents;

public sealed class InteractionsTests
{
	[Fact]
	public async Task EventSubscription_ToString_ShouldReturnExpectedValue()
	{
		EventSubscription interaction = new(3, "SomeEvent", this, Helper.GetMethodInfo());
		string expectedValue = "[3] subscribe to event SomeEvent";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task EventUnsubscription_ToString_ShouldReturnExpectedValue()
	{
		EventUnsubscription interaction = new(3, "SomeEvent", this, Helper.GetMethodInfo());
		string expectedValue = "[3] unsubscribe from event SomeEvent";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
	[Fact]
	public async Task Subscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockSubscribedTo<IMockVerify<int, Mock<int>>> subscribedTo = verify;
		interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		VerificationResult<IMockVerify<int, Mock<int>>> result = subscribedTo.Event("baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task Subscribed_WhenNameMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockSubscribedTo<IMockVerify<int, Mock<int>>> subscribedTo = verify;
		interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		VerificationResult<IMockVerify<int, Mock<int>>> result = subscribedTo.Event("foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task Subscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockSubscribedTo<IMockVerify<int, Mock<int>>> subscribedTo = verify;

		VerificationResult<IMockVerify<int, Mock<int>>> result = subscribedTo.Event("foo.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("subscribed to event bar");
	}
	[Fact]
	public async Task Unsubscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockUnsubscribedFrom<IMockVerify<int, Mock<int>>> unsubscribedFrom = verify;
		interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		VerificationResult<IMockVerify<int, Mock<int>>> result = unsubscribedFrom.Event("baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task Unsubscribed_WhenNameMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockUnsubscribedFrom<IMockVerify<int, Mock<int>>> unsubscribedFrom = verify;
		interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		VerificationResult<IMockVerify<int, Mock<int>>> result = unsubscribedFrom.Event("foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task Unsubscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1), "MyMock");
		IMockUnsubscribedFrom<IMockVerify<int, Mock<int>>> unsubscribedFrom = verify;

		VerificationResult<IMockVerify<int, Mock<int>>> result = unsubscribedFrom.Event("foo.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("unsubscribed from event bar");
	}
}
