#if NET10_0_OR_GREATER
using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsAnyRefStructOutTests
	{
		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IRefStructOutParameter<Span<int>> sut = It.IsAnyRefStructOut<Span<int>>();
			string expectedValue = "It.IsAnyRefStructOut<Span<int>>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task TryGetValue_ShouldReturnFalse()
		{
			IRefStructOutParameter<Span<int>> sut = It.IsAnyRefStructOut<Span<int>>();

			bool result = sut.TryGetValue(out _);

			await That(result).IsFalse();
		}
	}
}
#endif
