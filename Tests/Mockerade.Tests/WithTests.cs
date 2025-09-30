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
	public async Task WithMatching_CheckForNull_ShouldMatchForExpectedResult(int? value, bool expectedResult)
	{
		With.Parameter<int?> sut = With.Matching<int?>(v => v is null);

		bool result = sut.Matches(value);

		await That(result).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(42L)]
	[InlineData("foo")]
	public async Task WithMatching_DifferentType_ShouldNotMatch(object? value)
	{
		With.Parameter<int?> sut = With.Matching<int?>(_ => true);

		bool result = sut.Matches(value);

		await That(result).IsFalse();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task WithMatching_ShouldMatchForExpectedResult(bool predicateValue)
	{
		With.Parameter<string> sut = With.Matching<string>(_ => predicateValue);

		bool result = sut.Matches("foo");

		await That(result).IsEqualTo(predicateValue);
	}

	[Theory]
	[InlineData(42L)]
	[InlineData("foo")]
	public async Task WithOut_Invoked_ShouldAlwaysMatch(object? value)
	{
		With.InvokedOutParameter<int?> sut = With.Out<int?>();

		bool result = sut.Matches(value);

		await That(result).IsTrue();
	}

	[Theory]
	[InlineData(42L)]
	[InlineData("foo")]
	public async Task WithOut_ShouldAlwaysMatch(object? value)
	{
		With.OutParameter<int?> sut = With.Out<int?>(() => null);

		bool result = sut.Matches(value);

		await That(result).IsTrue();
	}

	[Theory]
	[InlineData(42)]
	[InlineData(-2)]
	public async Task WithOut_ShouldReturnValue(int? value)
	{
		With.OutParameter<int?> sut = With.Out(() => value);

		var result = sut.GetValue();

		await That(result).IsEqualTo(value);
	}

	[Theory]
	[InlineData(42L)]
	[InlineData("foo")]
	public async Task WithRef_Invoked_ShouldAlwaysMatch(object? value)
	{
		With.InvokedRefParameter<int?> sut = With.Ref<int?>();

		bool result = sut.Matches(value);

		await That(result).IsTrue();
	}

	[Theory]
	[InlineData(42L)]
	[InlineData("foo")]
	public async Task WithRef_DifferentType_ShouldNotMatch(object? value)
	{
		With.RefParameter<int?> sut = With.Ref<int?>(_ => true, _ => null);

		bool result = sut.Matches(value);

		await That(result).IsFalse();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task WithRef_ShouldMatchForExpectedResult(bool predicateValue)
	{
		With.RefParameter<string> sut = With.Ref<string>(_ => predicateValue, _ => "");

		bool result = sut.Matches("foo");

		await That(result).IsEqualTo(predicateValue);
	}

	[Theory]
	[InlineData(42)]
	[InlineData(-2)]
	public async Task WithRef_ShouldReturnValue(int? value)
	{
		With.RefParameter<int?> sut = With.Ref<int?>(v => v * 2);

		var result = sut.GetValue(value);

		await That(result).IsEqualTo(2 * value);
	}
}
