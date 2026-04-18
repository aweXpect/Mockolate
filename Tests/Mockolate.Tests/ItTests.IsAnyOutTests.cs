using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsAnyOutTests
	{
		[Fact]
		public async Task ShouldMatchInt()
		{
			IOutParameter<int?> sut = It.IsAnyOut<int?>();

			bool result = ((IParameterMatch<int?>)sut).Matches(123);

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ShouldMatchNull()
		{
			IOutParameter<int?> sut = It.IsAnyOut<int?>();

			bool result = ((IParameterMatch<int?>)sut).Matches(null);

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IOutParameter<int> sut = It.IsAnyOut<int>();
			string expectedValue = "It.IsAnyOut<int>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task TryGetValue_ShouldReturnFalse()
		{
			IOutParameter<int?> sut = It.IsAnyOut<int?>();

			bool result = sut.TryGetValue(out _);

			await That(result).IsFalse();
		}
	}
}
