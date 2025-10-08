using Mockolate.Checks;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Checks;

public sealed partial class MockInvokedTests
{
	[Fact]
	public async Task Method_WhenOnlyValueMatches_ShouldReturnNever()
	{
		var mockInteractions = new MockInteractions();
		IMockInteractions interactions = mockInteractions;
		IMockInvoked<Mock<int>> invoked = new MockInvoked<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [4]));

		var result = invoked.Method("baz.bar", With.Any<int>());

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WithoutInteractions_ShouldReturnNeverResult()
	{
		var interactions = new MockInteractions();
		IMockInvoked<Mock<int>> invoked = new MockInvoked<int, Mock<int>>(interactions, new MyMock<int>(1));

		var result = invoked.Method("foo.bar", With.Any<int>());

		await That(result).Never();
		await That(result.Expectation).IsEqualTo("invoked method bar(With.Any<int>())");
	}

	[Fact]
	public async Task Method_WhenNameAndValueMatches_ShouldReturnOnce()
	{
		var mockInteractions = new MockInteractions();
		IMockInteractions interactions = mockInteractions;
		IMockInvoked<Mock<int>> invoked = new MockInvoked<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [4]));

		var result = invoked.Method("foo.bar", With.Any<int>());

		await That(result).Once();
	}

	[Fact]
	public async Task Method_WhenOnlyNameMatches_ShouldReturnNever()
	{
		var mockInteractions = new MockInteractions();
		IMockInteractions interactions = mockInteractions;
		IMockInvoked<Mock<int>> invoked = new MockInvoked<int, Mock<int>>(mockInteractions, new MyMock<int>(1));
		interactions.RegisterInteraction(new MethodInvocation(0, "foo.bar", [4]));

		var result = invoked.Method("foo.bar", With.Any<string>());

		await That(result).Never();
	}
}
