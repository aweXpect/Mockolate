using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class VerifyGotIndexerTests
{
	[Fact]
	public async Task WhenNull_AndMatches_ShouldReturn()
	{
		IMyService mock = Mock.Create<IMyService>();
		_ = mock[null, null, null, null];

		await That(mock.VerifyMock.GotIndexer(It.IsAny<int?>(), null, It.IsNull<int?>(), It.IsAny<int?>())).Once();
	}

	[Fact]
	public async Task WhenParameterLengthDoesNotMatch_ShouldReturnNever()
	{
		IMyService mock = Mock.Create<IMyService>();
		_ = mock[1, 2];

		await That(mock.VerifyMock.GotIndexer(It.IsAny<int>())).Never();
		await That(mock.VerifyMock.GotIndexer(It.IsAny<int>(), It.IsAny<int>())).Once();
		await That(mock.VerifyMock.GotIndexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Never();
	}

	[Fact]
	public async Task WhenParametersDontMatch_ShouldReturnNever()
	{
		IMyService mock = Mock.Create<IMyService>();
		_ = mock[1, 2];

		await That(mock.VerifyMock.GotIndexer(It.Is(1), It.Is(2))).Once();
		await That(mock.VerifyMock.GotIndexer(It.Satisfies<int>(i => i != 1), It.Is(2))).Never();
		await That(mock.VerifyMock.GotIndexer(It.Is(1), It.Satisfies<int>(i => i != 2))).Never();
	}
}
