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
		public async Task ShouldMatchWhenPredicateReturnsTrue(bool expectedResult, params object?[] values)
		{
			IParameters sut = Parameters(_ => expectedResult);

			bool result = sut.Matches(values);

			await That(result).IsEqualTo(expectedResult);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameters sut = Parameters(_ => true);
			string expectedValue = "Parameters(_ => true)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
