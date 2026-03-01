using Mockolate.Internals;

namespace Mockolate.Internal.Tests;

public class WildcardTests
{
	[Test]
	[Arguments("\r\n")]
	[Arguments("\r")]
	[Arguments("\n")]
	public async Task ShouldSupportMultilineStrings(string newlineSeparator)
	{
		Wildcard wildcard = Wildcard.Pattern("fo*az", false);

		bool result = wildcard.Matches($"foo{newlineSeparator}bar{newlineSeparator}baz");

		await That(result).IsTrue();
	}
}
