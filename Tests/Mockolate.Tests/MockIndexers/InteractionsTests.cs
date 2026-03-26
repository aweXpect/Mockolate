using aweXpect.Chronology;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Tests.MockIndexers;

public sealed class InteractionsTests
{
	[Fact]
	public async Task IndexerGetterAccess_ToString_ShouldReturnExpectedValue()
	{
		IndexerGetterAccess interaction = new(4, [
			new NamedParameterValue("p1", "SomeProperty"),
			new NamedParameterValue("p2", 4),
			new NamedParameterValue("p3", null),
			new NamedParameterValue("p4", 150.Seconds()),
		]);
		string expectedValue = "[4] get indexer [SomeProperty, 4, null, 00:02:30]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess_ToString_ShouldReturnExpectedValue()
	{
		IndexerSetterAccess interaction = new(4, [
			new NamedParameterValue("p1", "SomeProperty"),
			new NamedParameterValue("p2", 4),
			new NamedParameterValue("p3", null),
			new NamedParameterValue("p4", 150.Seconds()),
		], 6);
		string expectedValue = "[4] set indexer [SomeProperty, 4, null, 00:02:30] to 6";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task IndexerSetterAccess_ToString_WithNull_ShouldReturnExpectedValue()
	{
		IndexerSetterAccess interaction = new(4, [
			new NamedParameterValue("p1", "SomeProperty"),
			new NamedParameterValue("p2", 4),
			new NamedParameterValue("p3", null),
			new NamedParameterValue("p4", 150.Seconds()),
		], null);
		string expectedValue = "[4] set indexer [SomeProperty, 4, null, 00:02:30] to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
