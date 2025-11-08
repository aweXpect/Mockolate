using Mockolate.Match;

namespace Mockolate.Tests;

public sealed partial class ParameterTests
{
	public sealed class WithAnyTests
	{
		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = WithAny<string>();
			string expectedValue = "WithAny<string>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("foo")]
		public async Task WithAny_ShouldAlwaysMatch(string? value)
		{
			IParameter<string> sut = WithAny<string>();

			bool result = sut.Matches(value);

			await That(result).IsTrue();
		}
	}
}
