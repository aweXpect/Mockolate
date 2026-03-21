using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class VerifySetIndexerTests
{
	[Fact]
	public async Task WhenNullParametersAndValue_AndMatches_ShouldReturn()
	{
		IMyService sut = IMyService.CreateMock();
		sut[null, null, null, null] = null;

		await That(sut.Mock.Verify[It.IsAny<int?>(), null, It.IsNull<int?>(), It.IsAny<int?>()].Set(null)).Once();
	}

	[Fact]
	public async Task WhenParameterLengthDoesNotMatch_ShouldReturnNever()
	{
		IMyService sut = IMyService.CreateMock();
		sut[1, 2] = "foo";

		await That(sut.Mock.Verify[It.IsAny<int>()].Set(It.Is("foo"))).Never();
		await That(sut.Mock.Verify[It.IsAny<int>(), It.IsAny<int>()].Set(It.Is("foo"))).Once();
		await That(sut.Mock.Verify[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()].Set(It.Is("foo"))).Never();
	}

	[Fact]
	public async Task WhenParametersDoNotMatch_ShouldReturnNever()
	{
		IMyService sut = IMyService.CreateMock();
		sut[1, 2] = "foo";

		await That(sut.Mock.Verify[It.Is(1), It.Is(2)].Set(It.Is("foo"))).Once();
		await That(sut.Mock.Verify[It.Satisfies<int>(i => i != 1), It.Is(2)].Set(It.Is("foo"))).Never();
		await That(sut.Mock.Verify[It.Is(1), It.Satisfies<int>(i => i != 2)].Set(It.Is("foo"))).Never();
	}
}
