using aweXpect.Chronology;
using Mockolate.Interactions;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class InteractionsTests
{
	[Fact]
	public async Task IndexerGetterAccess1_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerGetterAccess<string> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerGetterAccess<string>(
					"p1", "SomeProperty"));
		string expectedValue = "get indexer [SomeProperty]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerGetterAccess1_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerGetterAccess<string?> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerGetterAccess<string?>(
					"p1", null));
		string expectedValue = "get indexer [null]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerGetterAccess2_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerGetterAccess<string, int> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerGetterAccess<string, int>(
					"p1", "SomeProperty",
					"p2", 4));
		string expectedValue = "get indexer [SomeProperty, 4]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerGetterAccess2_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerGetterAccess<string?, long?> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerGetterAccess<string?, long?>(
					"p1", null,
					"p2", null));
		string expectedValue = "get indexer [null, null]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerGetterAccess3_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerGetterAccess<string, int, long?> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerGetterAccess<string, int, long?>(
					"p1", "SomeProperty",
					"p2", 4,
					"p3", 7L));
		string expectedValue = "get indexer [SomeProperty, 4, 7]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerGetterAccess3_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerGetterAccess<string?, int, long?> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerGetterAccess<string?, int, long?>(
					"p1", null,
					"p2", 4,
					"p3", null));
		string expectedValue = "get indexer [null, 4, null]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerGetterAccess4_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerGetterAccess<string, int, long?, TimeSpan> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerGetterAccess<string, int, long?, TimeSpan>(
					"p1", "SomeProperty",
					"p2", 4,
					"p3", 7L,
					"p4", 150.Seconds()));
		string expectedValue = "get indexer [SomeProperty, 4, 7, 00:02:30]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerGetterAccess4_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerGetterAccess<string, int, long?, TimeSpan> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerGetterAccess<string, int, long?, TimeSpan>(
					"p1", "SomeProperty",
					"p2", 4,
					"p3", null,
					"p4", 150.Seconds()));
		string expectedValue = "get indexer [SomeProperty, 4, null, 00:02:30]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerGetterAccess5_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerGetterAccess<string, int, long?, TimeSpan, bool> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerGetterAccess<string, int, long?, TimeSpan, bool>(
					"p1", "SomeProperty",
					"p2", 4,
					"p3", 7L,
					"p4", 150.Seconds(),
					"p5", true));
		string expectedValue = "get indexer [SomeProperty, 4, 7, 00:02:30, True]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerGetterAccess5_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerGetterAccess<string?, int, long?, TimeSpan, bool?> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerGetterAccess<string?, int, long?, TimeSpan, bool?>(
					"p1", null,
					"p2", 4,
					"p3", null,
					"p4", 150.Seconds(),
					"p5", null));
		string expectedValue = "get indexer [null, 4, null, 00:02:30, null]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess1_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess<string, int> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerSetterAccess<string, int>(
					"p1", "SomeProperty",
					6));
		string expectedValue = "set indexer [SomeProperty] to 6";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess1_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess<string?, string?> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerSetterAccess<string?, string?>(
					"p1", null,
					null));
		string expectedValue = "set indexer [null] to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess2_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess<string, int, int> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerSetterAccess<string, int, int>(
					"p1", "SomeProperty",
					"p2", 4,
					6));
		string expectedValue = "set indexer [SomeProperty, 4] to 6";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess2_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess<string?, long?, string?> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerSetterAccess<string?, long?, string?>(
					"p1", null,
					"p2", null,
					null));
		string expectedValue = "set indexer [null, null] to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess3_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess<string, int, long?, int> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerSetterAccess<string, int, long?, int>(
					"p1", "SomeProperty",
					"p2", 4,
					"p3", 7L,
					6));
		string expectedValue = "set indexer [SomeProperty, 4, 7] to 6";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess3_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess<string?, int, long?, string?> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerSetterAccess<string?, int, long?, string?>(
					"p1", null,
					"p2", 4,
					"p3", null,
					null));
		string expectedValue = "set indexer [null, 4, null] to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess4_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess<string, int, long?, TimeSpan, int> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerSetterAccess<string, int, long?, TimeSpan, int>(
					"p1", "SomeProperty",
					"p2", 4,
					"p3", 7L,
					"p4", 150.Seconds(),
					6));
		string expectedValue = "set indexer [SomeProperty, 4, 7, 00:02:30] to 6";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess4_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess<string, int, long?, TimeSpan, string?> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerSetterAccess<string, int, long?, TimeSpan, string?>(
					"p1", "SomeProperty",
					"p2", 4,
					"p3", null,
					"p4", 150.Seconds(),
					null));
		string expectedValue = "set indexer [SomeProperty, 4, null, 00:02:30] to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess5_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess<string, int, long?, TimeSpan, bool, int> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerSetterAccess<string, int, long?, TimeSpan, bool, int>(
					"p1", "SomeProperty",
					"p2", 4,
					"p3", 7L,
					"p4", 150.Seconds(),
					"p5", true,
					6));
		string expectedValue = "set indexer [SomeProperty, 4, 7, 00:02:30, True] to 6";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess5_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess<string?, int, long?, TimeSpan, bool?, string?> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerSetterAccess<string?, int, long?, TimeSpan, bool?, string?>(
					"p1", null,
					"p2", 4,
					"p3", null,
					"p4", 150.Seconds(),
					"p5", null,
					null));
		string expectedValue = "set indexer [null, 4, null, 00:02:30, null] to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
