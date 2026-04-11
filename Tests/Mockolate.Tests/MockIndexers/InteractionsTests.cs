using aweXpect.Chronology;
using Mockolate.Interactions;

namespace Mockolate.Tests.MockIndexers;

public sealed class InteractionsTests
{
	[Fact]
	public async Task IndexerGetterAccess_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerGetterAccess<string, int, long?, TimeSpan> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerGetterAccess<string, int, long?, TimeSpan>(
					"p1", "SomeProperty",
					"p2", 4,
					"p3", null,
					"p4", 150.Seconds()));
		string expectedValue = "get indexer [SomeProperty, 4, , 00:02:30]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess<string, int, long?, TimeSpan, int> interaction =
			((IMockInteractions)interactions).RegisterInteraction(
				new IndexerSetterAccess<string, int, long?, TimeSpan, int>(
					"p1", "SomeProperty",
					"p2", 4,
					"p3", null,
					"p4", 150.Seconds(),
					6));
		string expectedValue = "set indexer [SomeProperty, 4, , 00:02:30] to 6";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess_ToString_WithNull_ShouldReturnExpectedValue()
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
		string expectedValue = "set indexer [SomeProperty, 4, , 00:02:30] to ";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
