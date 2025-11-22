using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class VerifyGotIndexerTests
{
	[Test]
	public async Task WhenNull_AndMatches_ShouldReturn()
	{
		IMyService mock = Mock.Create<IMyService>();
		_ = mock[null, null, null, null];

		await That(mock.VerifyMock.GotIndexer(Any<int?>(), null, Null<int?>(), Any<int?>())).Once();
	}

	[Test]
	public async Task WhenParameterLengthDoesNotMatch_ShouldReturnNever()
	{
		IMyService mock = Mock.Create<IMyService>();
		_ = mock[1, 2];

		await That(mock.VerifyMock.GotIndexer(Any<int>())).Never();
		await That(mock.VerifyMock.GotIndexer(Any<int>(), Any<int>())).Once();
		await That(mock.VerifyMock.GotIndexer(Any<int>(), Any<int>(), Any<int>())).Never();
	}

	[Test]
	public async Task WhenParametersDontMatch_ShouldReturnNever()
	{
		IMyService mock = Mock.Create<IMyService>();
		_ = mock[1, 2];

		await That(mock.VerifyMock.GotIndexer(With(1), With(2))).Once();
		await That(mock.VerifyMock.GotIndexer(With<int>(i => i != 1), With(2))).Never();
		await That(mock.VerifyMock.GotIndexer(With(1), With<int>(i => i != 2))).Never();
	}
}
