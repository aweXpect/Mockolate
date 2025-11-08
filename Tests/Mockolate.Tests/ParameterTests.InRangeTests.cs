namespace Mockolate.Tests;

public sealed partial class ParameterTests
{
	public sealed class InRangeTests
	{
#if NET8_0_OR_GREATER
		[Theory]
		[InlineData(41, 43, true)]
		[InlineData(42, 42, false)]
		[InlineData(42, 44, false)]
		[InlineData(40, 42, false)]
		public async Task WithExclusive_ShouldMatchRangeExclusive(int minimum, int maximum, bool expectMatch)
		{
			IRangeParameter<int> sut = InRange(minimum, maximum).Exclusive();

			bool result = sut.Matches(42);

			await That(result).IsEqualTo(expectMatch);
		}
#endif

#if NET8_0_OR_GREATER
		[Fact]
		public async Task WhenMaximumIsLessThanMinimum_ShouldThrowArgumentOutOfRangeException()
		{
			void Act() =>
				_ = InRange(42.0, 41.0).Exclusive();

			await That(Act).Throws<ArgumentOutOfRangeException>()
				.WithMessage("The maximum must be greater than or equal to the minimum.").AsPrefix().And
				.WithParamName("maximum");
		}
#endif

#if NET8_0_OR_GREATER
		[Theory]
		[InlineData(41, 43, true)]
		[InlineData(42, 42, true)]
		[InlineData(43, 44, false)]
		[InlineData(40, 41, false)]
		public async Task ShouldMatchRangeInclusive(int minimum, int maximum, bool expectMatch)
		{
			IRangeParameter<int> sut = InRange(minimum, maximum);

			bool result = sut.Matches(42);

			await That(result).IsEqualTo(expectMatch);
		}
#endif

#if NET8_0_OR_GREATER
		[Fact]
		public async Task ToString_Exclusive_ShouldMatchExpectedValue()
		{
			IRangeParameter<int> sut = InRange(4, 2 * 3).Exclusive();
			string expectedValue = "InRange<int>(4, 2 * 3).Exclusive()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
#endif

#if NET8_0_OR_GREATER
		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IRangeParameter<int> sut = InRange(4, 2 * 3);
			string expectedValue = "InRange<int>(4, 2 * 3)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
#endif
	}
}
