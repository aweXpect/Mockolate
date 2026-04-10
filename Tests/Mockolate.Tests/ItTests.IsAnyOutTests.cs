using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsAnyOutTests
	{
		[Fact]
		public async Task ShouldNotMatchLong()
		{
			IOutParameter<int?> sut = It.IsAnyOut<int?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<long>(string.Empty, 42L));

			await That(result).IsFalse();
		}

		[Fact]
		public async Task ShouldNotMatchString()
		{
			IOutParameter<int?> sut = It.IsAnyOut<int?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<string>(string.Empty, "foo"));

			await That(result).IsFalse();
		}

		[Fact]
		public async Task ShouldMatchNull()
		{
			IOutParameter<int?> sut = It.IsAnyOut<int?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<int?>(string.Empty, null));

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ShouldMatchInt()
		{
			IOutParameter<int?> sut = It.IsAnyOut<int?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<int?>(string.Empty, 123));

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
	}
}
