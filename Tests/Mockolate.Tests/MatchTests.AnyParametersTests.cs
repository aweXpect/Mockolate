using System.Linq;
using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	public sealed class AnyParametersTests
	{
		[Theory]
		[InlineData(null, null)]
		[InlineData("", 1)]
		[InlineData("foo", null)]
		public async Task ShouldAlwaysMatch(params object?[] values)
		{
			IParameters sut = Match.AnyParameters();

			bool result = sut.Matches(values.Select(x => new NamedParameterValue(null, x)).ToArray());

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameters sut = Match.AnyParameters();
			string expectedValue = "Match.AnyParameters()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
