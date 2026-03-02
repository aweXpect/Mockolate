using Mockolate.Internals;

namespace Mockolate.Internal.Tests;

public class WildcardTests
{
	[Theory]
	[InlineData("\r\n")]
	[InlineData("\r")]
	[InlineData("\n")]
	public async Task ShouldSupportMultilineStrings(string newlineSeparator)
	{
		Wildcard wildcard = Wildcard.Pattern("fo*az", false);

		bool result = wildcard.Matches($"foo{newlineSeparator}bar{newlineSeparator}baz");

		await That(result).IsTrue();
	}
}
