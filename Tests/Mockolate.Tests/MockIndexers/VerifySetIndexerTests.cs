using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockIndexers;

public sealed class VerifySetIndexerTests
{
	[Fact]
	public async Task WhenNullParametersAndValue_AndMatches_ShouldReturn()
	{
		IMyService mock = Mock.Create<IMyService>();
		mock[null, null, null, null] = null;

		await That(mock.VerifyMock.SetIndexer(Any<int?>(), null, Null<int?>(), Any<int?>(), null)).Once();
	}

	[Fact]
	public async Task WhenParameterLengthDoesNotMatch_ShouldReturnNever()
	{
		IMyService mock = Mock.Create<IMyService>();
		mock[1, 2] = "foo";

		await That(mock.VerifyMock.SetIndexer(Any<int>(), With("foo"))).Never();
		await That(mock.VerifyMock.SetIndexer(Any<int>(), Any<int>(), With("foo"))).Once();
		await That(mock.VerifyMock.SetIndexer(Any<int>(), Any<int>(), Any<int>(), With("foo"))).Never();
	}

	[Fact]
	public async Task WhenParametersDoNotMatch_ShouldReturnNever()
	{
		IMyService mock = Mock.Create<IMyService>();
		mock[1, 2] = "foo";

		await That(mock.VerifyMock.SetIndexer(With(1), With(2), With("foo"))).Once();
		await That(mock.VerifyMock.SetIndexer(With<int>(i => i != 1), With(2), With("foo"))).Never();
		await That(mock.VerifyMock.SetIndexer(With(1), With<int>(i => i != 2), With("foo"))).Never();
	}
}
