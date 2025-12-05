using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	public sealed class AnyTests
	{
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("foo")]
		public async Task ShouldAlwaysMatch(string? value)
		{
			IParameter<string> sut = Any<string>();

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = Any<string>();
			string expectedValue = "Any<string>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
