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
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSubscribedTo<MockVerify<int, Mock<int>>> subscribedTo = new MockSubscribedTo<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		VerificationResult<MockVerify<int, Mock<int>>> result = subscribedTo.Event("baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task Subscribed_WhenNameMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSubscribedTo<MockVerify<int, Mock<int>>> subscribedTo = new MockSubscribedTo<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		VerificationResult<MockVerify<int, Mock<int>>> result = subscribedTo.Event("foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task Subscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSubscribedTo<MockVerify<int, Mock<int>>> subscribedTo = new MockSubscribedTo<int, Mock<int>>(verify);

		VerificationResult<MockVerify<int, Mock<int>>> result = subscribedTo.Event("foo.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("subscribed to event bar");
	}
	[Fact]
	public async Task Unsubscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockUnsubscribedFrom<MockVerify<int, Mock<int>>> unsubscribedFrom =
			new MockUnsubscribedFrom<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		VerificationResult<MockVerify<int, Mock<int>>> result = unsubscribedFrom.Event("baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task Unsubscribed_WhenNameMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockUnsubscribedFrom<MockVerify<int, Mock<int>>> unsubscribedFrom =
			new MockUnsubscribedFrom<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		VerificationResult<MockVerify<int, Mock<int>>> result = unsubscribedFrom.Event("foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task Unsubscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockUnsubscribedFrom<MockVerify<int, Mock<int>>> unsubscribedFrom =
			new MockUnsubscribedFrom<int, Mock<int>>(verify);

		VerificationResult<MockVerify<int, Mock<int>>> result = unsubscribedFrom.Event("foo.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("unsubscribed from event bar");
	}

	public sealed class ProtectedTests
	{
		[Fact]
		public async Task Unsubscribed_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			MockUnsubscribedFrom<int, Mock<int>> inner = new(verify);
			IMockUnsubscribedFrom<MockVerify<int, Mock<int>>> unsubscribedFrom = inner;
			IMockUnsubscribedFrom<MockVerify<int, Mock<int>>> @protected =
				new ProtectedMockUnsubscribedFrom<int, Mock<int>>(inner);
			interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, Helper.GetMethodInfo()));
			interactions.RegisterInteraction(new EventUnsubscription(1, "foo.bar", mock, Helper.GetMethodInfo()));

			VerificationResult<MockVerify<int, Mock<int>>> result1 = unsubscribedFrom.Event("foo.bar");
			VerificationResult<MockVerify<int, Mock<int>>> result2 = @protected.Event("foo.bar");

			await That(result1).Exactly(2);
			await That(result2).Exactly(2);
		}

		[Fact]
		public async Task Subscribed_ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			MockSubscribedTo<int, Mock<int>> inner = new(verify);
			IMockSubscribedTo<MockVerify<int, Mock<int>>> subscribedTo = inner;
			IMockSubscribedTo<MockVerify<int, Mock<int>>> @protected =
				new ProtectedMockSubscribedTo<int, Mock<int>>(inner);
			interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, Helper.GetMethodInfo()));
			interactions.RegisterInteraction(new EventSubscription(1, "foo.bar", mock, Helper.GetMethodInfo()));

			VerificationResult<MockVerify<int, Mock<int>>> result1 = subscribedTo.Event("foo.bar");
			VerificationResult<MockVerify<int, Mock<int>>> result2 = @protected.Event("foo.bar");

			await That(result1).Exactly(2);
			await That(result2).Exactly(2);
		}
	}
}
