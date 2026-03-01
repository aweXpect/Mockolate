using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class VerifySetIndexerTests
{
	[Test]
	public async Task WhenNullParametersAndValue_AndMatches_ShouldReturn()
	{
		IMyService mock = Mock.Create<IMyService>();
		mock[null, null, null, null] = null;

		await That(mock.VerifyMock.SetIndexer(It.IsAny<int?>(), null, It.IsNull<int?>(), It.IsAny<int?>(), null)).Once();
	}

	[Test]
	public async Task WhenParameterLengthDoesNotMatch_ShouldReturnNever()
	{
		IMyService mock = Mock.Create<IMyService>();
		mock[1, 2] = "foo";

		await That(mock.VerifyMock.SetIndexer(It.IsAny<int>(), It.Is("foo"))).Never();
		await That(mock.VerifyMock.SetIndexer(It.IsAny<int>(), It.IsAny<int>(), It.Is("foo"))).Once();
		await That(mock.VerifyMock.SetIndexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.Is("foo"))).Never();
	}

	[Test]
	public async Task WhenParametersDoNotMatch_ShouldReturnNever()
	{
		IMyService mock = Mock.Create<IMyService>();
		mock[1, 2] = "foo";

		await That(mock.VerifyMock.SetIndexer(It.Is(1), It.Is(2), It.Is("foo"))).Once();
		await That(mock.VerifyMock.SetIndexer(It.Satisfies<int>(i => i != 1), It.Is(2), It.Is("foo"))).Never();
		await That(mock.VerifyMock.SetIndexer(It.Is(1), It.Satisfies<int>(i => i != 2), It.Is("foo"))).Never();
	}
}
