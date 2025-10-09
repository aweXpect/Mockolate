using Mockolate.Interactions;

namespace Mockolate.Tests.Interactions;

public sealed class IndexerSetterAccessTests
{
	[Fact]
	public async Task ToString_ShouldReturnExpectedValue()
	{
		IndexerSetterAccess interaction = new(4, ["SomeProperty", 4, TimeSpan.FromSeconds(150)], 6);
		string expectedValue = "[4] set indexer [SomeProperty, 4, 00:02:30] to 6";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
