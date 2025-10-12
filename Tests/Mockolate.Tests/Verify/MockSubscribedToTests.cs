using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockSubscribedToTests
{
	[Fact]
	public async Task Subscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSubscribedTo<MockVerify<int, Mock<int>>> subscribedTo = new MockSubscribedTo<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		var result = subscribedTo.Event("baz.bar");

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

		var result = subscribedTo.Event("foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task Subscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSubscribedTo<MockVerify<int, Mock<int>>> subscribedTo = new MockSubscribedTo<int, Mock<int>>(verify);

		var result = subscribedTo.Event("foo.bar");

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("subscribed to event bar");
	}
}
