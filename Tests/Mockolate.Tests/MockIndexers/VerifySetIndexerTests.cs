using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class VerifySetIndexerTests
{
	[Fact]
	public async Task WhenNullParametersAndValue_AndMatches_ShouldReturn()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		mock.Subject[null, null, null, null] = null;

		await That(mock.Verify.SetIndexer(WithAny<int?>(), null, Null<int?>(), WithAny<int?>(), null)).Once();
	}

	[Fact]
	public async Task WhenParameterLengthDoesNotMatch_ShouldReturnNever()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		mock.Subject[1, 2] = "foo";

		await That(mock.Verify.SetIndexer(WithAny<int>(), With("foo"))).Never();
		await That(mock.Verify.SetIndexer(WithAny<int>(), WithAny<int>(), With("foo"))).Once();
		await That(mock.Verify.SetIndexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), With("foo"))).Never();
	}

	[Fact]
	public async Task WhenParametersDoNotMatch_ShouldReturnNever()
	{
		Mock<IMyService> mock = Mock.Create<IMyService>();
		mock.Subject[1, 2] = "foo";

		await That(mock.Verify.SetIndexer(With(1), With(2), With("foo"))).Once();
		await That(mock.Verify.SetIndexer(With<int>(i => i != 1), With(2), With("foo"))).Never();
		await That(mock.Verify.SetIndexer(With(1), With<int>(i => i != 2), With("foo"))).Never();
	}
}
