using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockIndexers;

public sealed class VerifyGotIndexerTests
{
	[Fact]
	public async Task WhenNull_AndMatches_ShouldReturn()
	{
		IMyService mock = Mock.Create<IMyService>();
		_ = mock[null, null, null, null];

		await That(mock.VerifyMock.GotIndexer(WithAny<int?>(), null, Null<int?>(), WithAny<int?>())).Once();
	}

	[Fact]
	public async Task WhenParameterLengthDoesNotMatch_ShouldReturnNever()
	{
		IMyService mock = Mock.Create<IMyService>();
		_ = mock[1, 2];

		await That(mock.VerifyMock.GotIndexer(WithAny<int>())).Never();
		await That(mock.VerifyMock.GotIndexer(WithAny<int>(), WithAny<int>())).Once();
		await That(mock.VerifyMock.GotIndexer(WithAny<int>(), WithAny<int>(), WithAny<int>())).Never();
	}

	[Fact]
	public async Task WhenParametersDontMatch_ShouldReturnNever()
	{
		IMyService mock = Mock.Create<IMyService>();
		_ = mock[1, 2];

		await That(mock.VerifyMock.GotIndexer(With(1), With(2))).Once();
		await That(mock.VerifyMock.GotIndexer(With<int>(i => i != 1), With(2))).Never();
		await That(mock.VerifyMock.GotIndexer(With(1), With<int>(i => i != 2))).Never();
	}
}
