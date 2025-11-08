using Mockolate.Match;

namespace Mockolate.Tests;

public sealed partial class ParameterTests
{
	public sealed class WithTests
	{
		[Fact]
		public async Task ToString_WithPredicate_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = With<string>(x => x.Length == 3);
			string expectedValue = "With<string>(x => x.Length == 3)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_WithValue_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = With("foo");
			string expectedValue = "\"foo\"";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_WithValueWithComparer_ShouldReturnExpectedValue()
		{
			IParameter<int> sut = With(4, new AllEqualComparer());
			string expectedValue = "With(4, new AllEqualComparer())";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(null, true)]
		[InlineData(1, false)]
		public async Task WithMatching_CheckForNull_ShouldMatchForExpectedResult(int? value, bool expectedResult)
		{
			IParameter<int?> sut = With<int?>(v => v is null);

			bool result = sut.Matches(value);

			await That(result).IsEqualTo(expectedResult);
		}

		[Theory]
		[InlineData(42L)]
		[InlineData("foo")]
		public async Task WithMatching_DifferentType_ShouldNotMatch(object? value)
		{
			IParameter<int?> sut = With<int?>(_ => true);

			bool result = sut.Matches(value);

			await That(result).IsFalse();
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task WithMatching_ShouldMatchForExpectedResult(bool predicateValue)
		{
			IParameter<string> sut = With<string>(_ => predicateValue);

			bool result = sut.Matches("foo");

			await That(result).IsEqualTo(predicateValue);
		}

		[Theory]
		[InlineData(null, true)]
		[InlineData("", false)]
		[InlineData("foo", false)]
		public async Task WithValue_Nullable_ShouldMatchWhenEqual(string? value, bool expectMatch)
		{
			IParameter<string?> sut = Null<string?>();

			bool result = sut.Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Theory]
		[InlineData(1, false)]
		[InlineData(5, true)]
		[InlineData(-5, false)]
		[InlineData(42, false)]
		public async Task WithValue_ShouldMatchWhenEqual(int value, bool expectMatch)
		{
			IParameter<int> sut = With(5);

			bool result = sut.Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(-42)]
		public async Task WithValue_WithComparer_ShouldUseComparer(int value)
		{
			IParameter<int> sut = With(5, new AllEqualComparer());

			bool result = sut.Matches(value);

			await That(result).IsTrue();
		}
	}
}
