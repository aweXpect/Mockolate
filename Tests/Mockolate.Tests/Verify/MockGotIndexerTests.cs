﻿using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockGotIndexerTests
{
	[Fact]
	public async Task WhenParametersDontMatch_ShouldReturnNever()
	{
		var mock = Mock.Create<IMyService>();
		_ = mock.Subject[1, 2];

		await That(mock.Verify.GotIndexer(1, 2)).Once();
		await That(mock.Verify.GotIndexer(With.Matching<int>(i => i != 1), 2)).Never();
		await That(mock.Verify.GotIndexer(1, With.Matching<int>(i => i != 2))).Never();
	}

	[Fact]
	public async Task WhenParameterLengthDoesNotMatch_ShouldReturnNever()
	{
		var mock = Mock.Create<IMyService>();
		_ = mock.Subject[1, 2];

		await That(mock.Verify.GotIndexer(With.Any<int>())).Never();
		await That(mock.Verify.GotIndexer(With.Any<int>(), With.Any<int>())).Once();
		await That(mock.Verify.GotIndexer(With.Any<int>(), With.Any<int>(), With.Any<int>())).Never();
	}

	[Fact]
	public async Task WhenNull_AndMatches_ShouldReturn()
	{
		var mock = Mock.Create<IMyService>();
		_ = mock.Subject[null, null, null, null];

		await That(mock.Verify.GotIndexer(With.Any<int?>(), null, With.Null<int?>(), With.Any<int?>())).Once();
	}

}
