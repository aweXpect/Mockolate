using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockGotTests
{
	[Fact]
	public async Task WhenNameDoesNotMatch_ShouldReturnNever()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockGot<Mock<int>> mockGot = new MockGot<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new PropertyGetterAccess(0, "foo.bar"));

		CheckResult<Mock<int>> result = mockGot.Property("baz.bar");

		await That(result.Never());
	}

	[Fact]
	public async Task WhenNameMatches_ShouldReturnOnce()
	{
		MockInteractions mockInteractions = new();
		IMockInteractions interactions = mockInteractions;
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockGot<Mock<int>> mockGot = new MockGot<int, Mock<int>>(verify);
		interactions.RegisterInteraction(new PropertyGetterAccess(0, "foo.bar"));

		CheckResult<Mock<int>> result = mockGot.Property("foo.bar");

		await That(result.Once());
	}

	[Fact]
	public async Task WithoutInteractions_ShouldReturnNeverResult()
	{
		MockInteractions mockInteractions = new();
		MockVerify<int, Mock<int>> verify = new(mockInteractions, new MyMock<int>(1));
		IMockGot<Mock<int>> mockGot = new MockGot<int, Mock<int>>(verify);

		CheckResult<Mock<int>> result = mockGot.Property("foo.bar");

		await That(result.Never());
		await That(result.Expectation).IsEqualTo("got property bar");
	}
}
