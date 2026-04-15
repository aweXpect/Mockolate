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
			new NamedParameterValue<string>("p1", "SomeProperty"),
			new NamedParameterValue<int>("p2", 4),
			new NamedParameterValue<long?>("p3", null),
			new NamedParameterValue<TimeSpan>("p4", 150.Seconds()),
		]));
		string expectedValue = "get indexer [SomeProperty, 4, null, 00:02:30]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess interaction = ((IMockInteractions)interactions).RegisterInteraction(new IndexerSetterAccess([
			new NamedParameterValue<string>("p1", "SomeProperty"),
			new NamedParameterValue<int>("p2", 4),
			new NamedParameterValue<long?>("p3", null),
			new NamedParameterValue<TimeSpan>("p4", 150.Seconds()),
		], new NamedParameterValue<int>("value", 6)));
		string expectedValue = "set indexer [SomeProperty, 4, null, 00:02:30] to 6";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		IndexerSetterAccess interaction = ((IMockInteractions)interactions).RegisterInteraction(new IndexerSetterAccess([
			new NamedParameterValue<string>("p1", "SomeProperty"),
			new NamedParameterValue<int>("p2", 4),
			new NamedParameterValue<long?>("p3", null),
			new NamedParameterValue<TimeSpan>("p4", 150.Seconds()),
		], new NamedParameterValue<string?>("value", null)));
		string expectedValue = "set indexer [SomeProperty, 4, null, 00:02:30] to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
