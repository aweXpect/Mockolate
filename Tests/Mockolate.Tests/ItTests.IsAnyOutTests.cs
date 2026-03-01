using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsAnyOutTests
	{
		[Test]
		[Arguments(42L, false)]
		[Arguments("foo", false)]
		[Arguments(null, true)]
		[Arguments(123, true)]
		public async Task ShouldCheckType(object? value, bool expectMatch)
		{
			IOutParameter<int?> sut = It.IsAnyOut<int?>();

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Test]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IOutParameter<int> sut = It.IsAnyOut<int>();
			string expectedValue = "It.IsAnyOut<int>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
