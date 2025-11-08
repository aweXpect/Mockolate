namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	public sealed class WithAnyParametersTests
	{
		[Theory]
		[InlineData(null, null)]
		[InlineData("", 1)]
		[InlineData("foo", null)]
		public async Task ShouldAlwaysMatch(params object?[] values)
		{
			IParameters sut = WithAnyParameters();

			bool result = sut.Matches(values);

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameters sut = WithAnyParameters();
			string expectedValue = "WithAnyParameters()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
