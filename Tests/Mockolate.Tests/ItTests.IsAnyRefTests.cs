using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsAnyRefTests
	{
		[Fact]
		public async Task ShouldMatchNull()
		{
			IRefParameter<int?> sut = It.IsAnyRef<int?>();

			bool result = ((IParameterMatch<int?>)sut).Matches(null);

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ShouldMatchInt()
		{
			IRefParameter<int?> sut = It.IsAnyRef<int?>();

			bool result = ((IParameterMatch<int?>)sut).Matches(123);

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IRefParameter<int> sut = It.IsAnyRef<int>();
			string expectedValue = "It.IsAnyRef<int>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
