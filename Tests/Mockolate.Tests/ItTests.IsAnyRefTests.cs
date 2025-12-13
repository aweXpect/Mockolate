using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsAnyRefTests
	{
		[Test]
		[Arguments(42L, false)]
		[Arguments("foo", false)]
		[Arguments(null, true)]
		[Arguments(123, true)]
		public async Task ShouldCheckType(object? value, bool expectMatch)
		{
			IRefParameter<int?> sut = It.IsAnyRef<int?>();

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Test]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IRefParameter<int> sut = It.IsAnyRef<int>();
			string expectedValue = "It.IsAnyRef<int>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
