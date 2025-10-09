using Mockolate.Interactions;

namespace Mockolate.Tests.Interactions;

public sealed class PropertySetterAccessTests
{
	[Fact]
	public async Task ToString_ShouldReturnExpectedValue()
	{
		PropertySetterAccess interaction = new(3, "SomeProperty", 5);
		string expectedValue = "[3] set property SomeProperty to 5";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task ToString_WithNull_ShouldReturnExpectedValue()
	{
		PropertySetterAccess interaction = new(4, "SomeProperty", null);
		string expectedValue = "[4] set property SomeProperty to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
