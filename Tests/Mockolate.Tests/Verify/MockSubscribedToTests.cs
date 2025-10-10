using System.Reflection;
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
		IMockSubscribedTo<Mock<int>> subscribedTo = new MockSubscribedTo<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		VerificationResult<Mock<int>> result = subscribedTo.Event("baz.bar");

		result.Never();
	}

	[Fact]
	public async Task Subscribed_WhenNameMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSubscribedTo<Mock<int>> subscribedTo = new MockSubscribedTo<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new EventSubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		VerificationResult<Mock<int>> result = subscribedTo.Event("foo.bar");

		result.Once();
	}

	[Fact]
	public async Task Subscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockSubscribedTo<Mock<int>> subscribedTo = new MockSubscribedTo<int, Mock<int>>(verify);

		VerificationResult<Mock<int>> result = subscribedTo.Event("foo.bar");

		result.Never();
		await That(result.Expectation).IsEqualTo("subscribed to event bar");
	}
}
