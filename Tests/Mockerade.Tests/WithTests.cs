namespace Mockerade.Tests;

public sealed class WithTests
{
	[Theory]
	[InlineData(null, false)]
	[InlineData("", false)]
	[InlineData("foo", true)]
	[InlineData("fo", false)]
	public async Task ImplicitConversion_ShouldCheckForEquality(string? value, bool expectMatch)
	{
		With.Parameter<string> sut = "foo";

		bool result = sut.Matches(value);

		await That(result).IsEqualTo(expectMatch);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("foo")]
	public async Task WithAny_ShouldAlwaysMatch(string? value)
	{
		With.Parameter<string> sut = With.Any<string>();

		bool result = sut.Matches(value);

		await That(result).IsTrue();
	}

	[Theory]
	[InlineData(null, true)]
	[InlineData(1, false)]
	public async Task WithMatching_CheckForNull_ShouldReturnExpectedResult(int? value, bool expectedResult)
	{
		With.Parameter<int?> sut = With.Matching<int?>(v => v is null);

		bool result = sut.Matches(value);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(42L)]
	[InlineData("foo")]
	public async Task WithMatching_DifferentType_ShouldReturnFalse(object? value)
	{
		With.Parameter<int?> sut = With.Matching<int?>(_ => true);

		bool result = sut.Matches(value);

		await That(result).IsFalse();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task WithMatching_ShouldReturnPredicateValue(bool predicateValue)
	{
		With.Parameter<string> sut = With.Matching<string>(_ => predicateValue);

		bool result = sut.Matches("foo");

		await That(result).IsEqualTo(predicateValue);
	}
}
