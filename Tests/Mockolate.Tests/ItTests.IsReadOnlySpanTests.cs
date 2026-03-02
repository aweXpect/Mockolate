#if NET8_0_OR_GREATER
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsReadOnlySpanTests
	{
		[Theory]
		[InlineData("", false)]
		[InlineData("foo", false)]
		[InlineData("bar", true)]
		public async Task ShouldAlwaysMatch(string value, bool expectSuccess)
		{
			ReadOnlySpan<char> valueSpan = value.AsSpan();
			IVerifyReadOnlySpanParameter<char> sut = It.IsReadOnlySpan<char>(c => c.Length > 0 && c[0] == 'b');

			bool result = ((IParameter)sut).Matches((ReadOnlySpanWrapper<char>)valueSpan);

			await That(result).IsEqualTo(expectSuccess);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IVerifyReadOnlySpanParameter<char> sut = It.IsReadOnlySpan<char>(c => c[0] == 'a');
			string expectedValue = "It.IsReadOnlySpan<char>(c => c[0] == 'a')";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
#endif
