using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	public sealed class AnyOutTests
	{
		[Theory]
		[InlineData(42L, false)]
		[InlineData("foo", false)]
		[InlineData(null, true)]
		[InlineData(123, true)]
		public async Task ShouldCheckType(object? value, bool expectMatch)
		{
			IOutParameter<int?> sut = AnyOut<int?>();

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IOutParameter<int> sut = AnyOut<int>();
			string expectedValue = "AnyOut<int>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
