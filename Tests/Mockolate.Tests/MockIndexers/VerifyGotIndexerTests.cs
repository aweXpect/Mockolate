using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class VerifyGotIndexerTests
{
	[Fact]
	public async Task WhenNull_AndMatches_ShouldReturn()
	{
		IMyService mock = IMyService.CreateMock();
		_ = mock[null, null, null, null];

		await That(mock.Mock.Verify[It.IsAny<int?>(), null, It.IsNull<int?>(), It.IsAny<int?>()].Got()).Once();
	}

	[Fact]
	public async Task WhenParameterLengthDoesNotMatch_ShouldReturnNever()
	{
		IMyService mock = IMyService.CreateMock();
		_ = mock[1, 2];

		await That(mock.Mock.Verify[It.IsAny<int>()].Got()).Never();
		await That(mock.Mock.Verify[It.IsAny<int>(), It.IsAny<int>()].Got()).Once();
		await That(mock.Mock.Verify[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()].Got()).Never();
	}

	[Fact]
	public async Task WhenParametersDontMatch_ShouldReturnNever()
	{
		IMyService mock = IMyService.CreateMock();
		_ = mock[1, 2];

		await That(mock.Mock.Verify[It.Is(1), It.Is(2)].Got()).Once();
		await That(mock.Mock.Verify[It.Satisfies<int>(i => i != 1), It.Is(2)].Got()).Never();
		await That(mock.Mock.Verify[It.Is(1), It.Satisfies<int>(i => i != 2)].Got()).Never();
	}
}
