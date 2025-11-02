using Mockolate.Interactions;

namespace Mockolate.Tests.MockIndexers;

public sealed class IndexerGetterAccessTests
{
	[Fact]
	public async Task ToString_ShouldReturnExpectedValue()
	{
		IndexerGetterAccess interaction = new(4, ["SomeProperty", 4, null, TimeSpan.FromSeconds(150),]);
		string expectedValue = "[4] get indexer [SomeProperty, 4, null, 00:02:30]";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
