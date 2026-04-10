using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsAnyRefTests
	{
		[Fact]
		public async Task ShouldNotMatchLong()
		{
			IRefParameter<int?> sut = It.IsAnyRef<int?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<long>(string.Empty, 42L));

			await That(result).IsFalse();
		}

		[Fact]
		public async Task ShouldNotMatchString()
		{
			IRefParameter<int?> sut = It.IsAnyRef<int?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<string>(string.Empty, "foo"));

			await That(result).IsFalse();
		}

		[Fact]
		public async Task ShouldMatchNull()
		{
			IRefParameter<int?> sut = It.IsAnyRef<int?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<int?>(string.Empty, null));

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ShouldMatchInt()
		{
			IRefParameter<int?> sut = It.IsAnyRef<int?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<int?>(string.Empty, 123));

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
