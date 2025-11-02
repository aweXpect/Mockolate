using Mockolate.Interactions;

namespace Mockolate.Tests.Interactions;

public sealed class PropertyGetterAccessTests
{
	[Fact]
	public async Task ToString_ShouldReturnExpectedValue()
	{
		PropertyGetterAccess interaction = new(3, "SomeProperty");
		string expectedValue = "[3] get property SomeProperty";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
