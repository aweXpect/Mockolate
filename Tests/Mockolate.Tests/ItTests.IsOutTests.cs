using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsOutTests
	{
		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IOutParameter<int> sut = It.IsOut(() => 3);
			string expectedValue = "It.IsOut<int>(() => 3)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_Verify_ShouldReturnExpectedValue()
		{
			IVerifyOutParameter<int> sut = It.IsOut<int>();
			string expectedValue = "It.IsOut<int>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(42)]
		[InlineData(-2)]
		public async Task WithOut_ShouldReturnValue(int? value)
		{
			IOutParameter<int?> sut = It.IsOut(() => value);

			bool found = sut.TryGetValue(out int? result);

			await That(found).IsTrue();
			await That(result).IsEqualTo(value);
		}

		[Fact]
		public async Task WithOut_Verify_ShouldAlwaysMatch()
		{
			IVerifyOutParameter<int?> sut = It.IsOut<int?>();

			bool result = ((IParameterMatch<int?>)sut).Matches(42);

			await That(result).IsTrue();
			await That(() => ((IParameterMatch<int?>)sut).InvokeCallbacks(0)).DoesNotThrow();
		}
	}
}
