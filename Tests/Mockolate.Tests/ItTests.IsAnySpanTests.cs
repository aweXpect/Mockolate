#if NET8_0_OR_GREATER
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsAnySpanTests
	{
		[Theory]
		[InlineData("")]
		[InlineData("foo")]
		[InlineData("bar")]
		public async Task ShouldAlwaysMatch(string value)
		{
			Span<char> valueSpan = new(value.ToCharArray());
			IVerifySpanParameter<char> sut = It.IsAnySpan<char>();

			bool result = ((IParameter)sut).Matches((SpanWrapper<char>)valueSpan);

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IVerifySpanParameter<char> sut = It.IsAnySpan<char>();
			string expectedValue = "It.IsAnySpan<char>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
#endif
