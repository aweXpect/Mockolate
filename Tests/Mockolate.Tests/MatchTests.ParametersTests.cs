using System.Linq;
using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	public sealed class ParametersTests
	{
		[Test]
		[Arguments(true, null, null)]
		[Arguments(true, "", 1)]
		[Arguments(true, "foo", null)]
		[Arguments(false, null, null)]
		[Arguments(false, "", 1)]
		[Arguments(false, "foo", null)]
		public async Task ShouldMatchWhenPredicateReturnsTrue(bool expectedResult, params object?[] values)
		{
			IParameters sut = Match.Parameters(_ => expectedResult);

			bool result = sut.Matches(values.Select(x => new NamedParameterValue(null, x)).ToArray());

			await That(result).IsEqualTo(expectedResult);
		}

		[Test]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameters sut = Match.Parameters(_ => true);
			string expectedValue = "Match.Parameters(_ => true)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
