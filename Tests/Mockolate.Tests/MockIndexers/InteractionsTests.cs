using aweXpect.Chronology;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Tests.MockIndexers;

public sealed class InteractionsTests
{
	[Fact]
	public async Task IndexerGetterAccess_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerGetterAccess interaction = ((IMockInteractions)interactions).RegisterInteraction(new IndexerGetterAccess([
			new NamedParameterValue("p1", "SomeProperty"),
			new NamedParameterValue("p2", 4),
			new NamedParameterValue("p3", null),
			new NamedParameterValue("p4", (TimeSpan)150.Seconds()),
		]));
		string expectedValue = "[0] get indexer [SomeProperty, 4, null, 00:02:30]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess interaction = ((IMockInteractions)interactions).RegisterInteraction(new IndexerSetterAccess([
			new NamedParameterValue("p1", "SomeProperty"),
			new NamedParameterValue("p2", 4),
			new NamedParameterValue("p3", null),
			new NamedParameterValue("p4", (TimeSpan)150.Seconds()),
		], 6));
		string expectedValue = "[0] set indexer [SomeProperty, 4, null, 00:02:30] to 6";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess interaction = ((IMockInteractions)interactions).RegisterInteraction(new IndexerSetterAccess([
			new NamedParameterValue("p1", "SomeProperty"),
			new NamedParameterValue("p2", 4),
			new NamedParameterValue("p3", null),
			new NamedParameterValue("p4", (TimeSpan)150.Seconds()),
		], null));
		string expectedValue = "[0] set indexer [SomeProperty, 4, null, 00:02:30] to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
