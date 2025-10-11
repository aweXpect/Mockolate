using System.Reflection;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockUnsubscribedFromTests
{
	[Fact]
	public async Task Unsubscribed_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockUnsubscribedFrom<Mock<int>> unsubscribedFrom = new MockUnsubscribedFrom<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		VerificationResult<Mock<int>> result = unsubscribedFrom.Event("baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task Unsubscribed_WhenNameMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockUnsubscribedFrom<Mock<int>> unsubscribedFrom = new MockUnsubscribedFrom<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new EventUnsubscription(0, "foo.bar", this, Helper.GetMethodInfo()));

		VerificationResult<Mock<int>> result = unsubscribedFrom.Event("foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task Unsubscribed_WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockUnsubscribedFrom<Mock<int>> unsubscribedFrom = new MockUnsubscribedFrom<int, Mock<int>>(verify);

		VerificationResult<Mock<int>> result = unsubscribedFrom.Event("foo.bar");

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("unsubscribed from event bar");
	}
}
