using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockSetIndexerTests
{
	[Fact]
	public async Task WhenParametersDontMatch_ShouldReturnNever()
	{
		var mock = Mock.Create<IMyService>();
		mock.Subject[1, 2] = "foo";

		mock.Verify.SetIndexer(1, 2, "foo").Once();
		mock.Verify.SetIndexer(With.Matching<int>(i => i != 1), 2, "foo").Never();
		mock.Verify.SetIndexer(1, With.Matching<int>(i => i != 2), "foo").Never();
	}

	[Fact]
	public async Task WhenParameterLengthDoesNotMatch_ShouldReturnNever()
	{
		var mock = Mock.Create<IMyService>();
		mock.Subject[1, 2] = "foo";

		mock.Verify.SetIndexer(With.Any<int>(), "foo").Never();
		mock.Verify.SetIndexer(With.Any<int>(), With.Any<int>(), "foo").Once();
		mock.Verify.SetIndexer(With.Any<int>(), With.Any<int>(), With.Any<int>(), "foo").Never();
	}

	[Fact]
	public async Task WhenNull_AndMatches_ShouldReturn()
	{
		var mock = Mock.Create<IMyService>();
		mock.Subject[null, null, null, null] = "foo";

		mock.Verify.SetIndexer(With.Any<int?>(), null, With.Null<int?>(), With.Any<int?>(), "foo").Once();
	}

}
