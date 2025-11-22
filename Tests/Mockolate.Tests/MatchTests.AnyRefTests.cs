namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	public sealed class AnyRefTests
	{
		[Theory]
		[InlineData(42L, false)]
		[InlineData("foo", false)]
		[InlineData(null, true)]
		[InlineData(123, true)]
		public async Task ShouldCheckType(object? value, bool expectMatch)
		{
			IRefParameter<int?> sut = AnyRef<int?>();

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IRefParameter<int> sut = AnyRef<int>();
			string expectedValue = "AnyRef<int>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
