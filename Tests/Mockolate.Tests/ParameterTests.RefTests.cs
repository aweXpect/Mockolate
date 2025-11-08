using Mockolate.Match;

namespace Mockolate.Tests;

public sealed partial class ParameterTests
{
	public sealed class RefTests
	{
		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IRefParameter<int?> sut = Ref<int?>(v => v * 3);
			string expectedValue = "Ref<int?>(v => v * 3)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_Verify_ShouldReturnExpectedValue()
		{
			IVerifyRefParameter<int> sut = Ref<int>();
			string expectedValue = "Ref<int>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_WithPredicate_ShouldReturnExpectedValue()
		{
			IRefParameter<int?> sut = Ref<int?>(v => v > 3, v => v * 3);
			string expectedValue = "Ref<int?>(v => v > 3, v => v * 3)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(42L)]
		[InlineData("foo")]
		public async Task WithRef_DifferentType_ShouldNotMatch(object? value)
		{
			IRefParameter<int?> sut = Ref<int?>(_ => true, _ => null);

			bool result = sut.Matches(value);

			await That(result).IsFalse();
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task WithRef_ShouldMatchForExpectedResult(bool predicateValue)
		{
			IRefParameter<string> sut = Ref<string>(_ => predicateValue, _ => "");

			bool result = sut.Matches("foo");

			await That(result).IsEqualTo(predicateValue);
		}

		[Theory]
		[InlineData(42)]
		[InlineData(-2)]
		public async Task WithRef_ShouldReturnValue(int? value)
		{
			IRefParameter<int?> sut = Ref<int?>(v => v * 2);

			int? result = sut.GetValue(value);

			await That(result).IsEqualTo(2 * value);
		}

		[Theory]
		[InlineData(42L)]
		[InlineData("foo")]
		public async Task WithRef_Verify_ShouldAlwaysMatch(object? value)
		{
			IVerifyRefParameter<int?> sut = Ref<int?>();

			bool result = sut.Matches(value);

			await That(result).IsTrue();
		}
	}
}
