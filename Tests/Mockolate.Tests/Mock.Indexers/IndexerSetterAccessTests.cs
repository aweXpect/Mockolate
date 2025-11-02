using Mockolate.Interactions;

namespace Mockolate.Tests.Interactions;

public sealed class IndexerSetterAccessTests
{
	[Fact]
	public async Task ToString_ShouldReturnExpectedValue()
	{
		IndexerSetterAccess interaction = new(4, ["SomeProperty", 4, null, TimeSpan.FromSeconds(150),], 6);
		string expectedValue = "[4] set indexer [SomeProperty, 4, null, 00:02:30] to 6";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task ToString_WithNull_ShouldReturnExpectedValue()
	{
		IndexerSetterAccess interaction = new(4, ["SomeProperty", 4, null, TimeSpan.FromSeconds(150),], null);
		string expectedValue = "[4] set indexer [SomeProperty, 4, null, 00:02:30] to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
