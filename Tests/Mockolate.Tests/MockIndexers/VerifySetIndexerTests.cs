using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class VerifySetIndexerTests
{
	[Fact]
	public async Task WhenNullParametersAndValue_AndMatches_ShouldReturn()
	{
		IMyService mock = IMyService.CreateMock();
		mock[null, null, null, null] = null;

		await That(mock.Mock.Verify[It.IsAny<int?>(), null, It.IsNull<int?>(), It.IsAny<int?>()].Set(null)).Once();
	}

	[Fact]
	public async Task WhenParameterLengthDoesNotMatch_ShouldReturnNever()
	{
		IMyService mock = IMyService.CreateMock();
		mock[1, 2] = "foo";

		await That(mock.Mock.Verify[It.IsAny<int>()].Set(It.Is("foo"))).Never();
		await That(mock.Mock.Verify[It.IsAny<int>(), It.IsAny<int>()].Set(It.Is("foo"))).Once();
		await That(mock.Mock.Verify[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()].Set(It.Is("foo"))).Never();
	}

	[Fact]
	public async Task WhenParametersDoNotMatch_ShouldReturnNever()
	{
		IMyService mock = IMyService.CreateMock();
		mock[1, 2] = "foo";

		await That(mock.Mock.Verify[It.Is(1), It.Is(2)].Set(It.Is("foo"))).Once();
		await That(mock.Mock.Verify[It.Satisfies<int>(i => i != 1), It.Is(2)].Set(It.Is("foo"))).Never();
		await That(mock.Mock.Verify[It.Is(1), It.Satisfies<int>(i => i != 2)].Set(It.Is("foo"))).Never();
	}
}
