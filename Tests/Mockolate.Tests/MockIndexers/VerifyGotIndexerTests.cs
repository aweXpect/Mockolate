using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class VerifyGotIndexerTests
{
	[Fact]
	public async Task WhenNull_AndMatches_ShouldReturn()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		_ = mock.Subject[null, null, null, null];

		await That(mock.Verify.GotIndexer(WithAny<int?>(), null, Null<int?>(), WithAny<int?>())).Once();
	}

	[Fact]
	public async Task WhenParameterLengthDoesNotMatch_ShouldReturnNever()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		_ = mock.Subject[1, 2];

		await That(mock.Verify.GotIndexer(WithAny<int>())).Never();
		await That(mock.Verify.GotIndexer(WithAny<int>(), WithAny<int>())).Once();
		await That(mock.Verify.GotIndexer(WithAny<int>(), WithAny<int>(), WithAny<int>())).Never();
	}

	[Fact]
	public async Task WhenParametersDontMatch_ShouldReturnNever()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		_ = mock.Subject[1, 2];

		await That(mock.Verify.GotIndexer(With(1), With(2))).Once();
		await That(mock.Verify.GotIndexer(With<int>(i => i != 1), With(2))).Never();
		await That(mock.Verify.GotIndexer(With(1), With<int>(i => i != 2))).Never();
	}
}
