#if NET8_0_OR_GREATER
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsAnyReadOnlySpanTests
	{
		[Theory]
		[InlineData("")]
		[InlineData("foo")]
		[InlineData("bar")]
		public async Task ShouldAlwaysMatch(string value)
		{
			ReadOnlySpan<char> valueSpan = value.AsSpan();
			IVerifyReadOnlySpanParameter<char> sut = It.IsAnyReadOnlySpan<char>();

			bool result = ((IParameter)sut).Matches((ReadOnlySpanWrapper<char>)valueSpan);

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IVerifyReadOnlySpanParameter<char> sut = It.IsAnyReadOnlySpan<char>();
			string expectedValue = "It.IsAnyReadOnlySpan<char>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
#endif
