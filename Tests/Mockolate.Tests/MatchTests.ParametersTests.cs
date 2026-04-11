using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	public sealed class ParametersTests
	{
		[Theory]
		[InlineData(true, null, null)]
		[InlineData(true, "", 1)]
		[InlineData(true, "foo", null)]
		[InlineData(false, null, null)]
		[InlineData(false, "", 1)]
		[InlineData(false, "foo", null)]
		public async Task ShouldMatchWhenPredicateReturnsTrue(bool expectedResult, string? value1, int? value2)
		{
			IParameters sut = Match.Parameters(_ => expectedResult);
			IParametersMatch match = (IParametersMatch)sut;

			bool result = match.Matches([value1, value2,]);

			await That(result).IsEqualTo(expectedResult);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameters sut = Match.Parameters(_ => true);
			string expectedValue = "Match.Parameters(_ => true)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
