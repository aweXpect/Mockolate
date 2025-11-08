using Mockolate.Match;

namespace Mockolate.Tests;

public sealed partial class ParameterTests
{
	public sealed class OutTests
	{
		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IOutParameter<int> sut = Out(() => 3);
			string expectedValue = "Out<int>(() => 3)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_Verify_ShouldReturnExpectedValue()
		{
			IVerifyOutParameter<int> sut = Out<int>();
			string expectedValue = "Out<int>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(42L)]
		[InlineData("foo")]
		public async Task WithOut_ShouldAlwaysMatch(object? value)
		{
			IOutParameter<int?> sut = Out<int?>(() => null);

			bool result = sut.Matches(value);

			await That(result).IsTrue();
		}

		[Theory]
		[InlineData(42)]
		[InlineData(-2)]
		public async Task WithOut_ShouldReturnValue(int? value)
		{
			IOutParameter<int?> sut = Out(() => value);

			int? result = sut.GetValue();

			await That(result).IsEqualTo(value);
		}

		[Theory]
		[InlineData(42L)]
		[InlineData("foo")]
		public async Task WithOut_Verify_ShouldAlwaysMatch(object? value)
		{
			IVerifyOutParameter<int?> sut = Out<int?>();

			bool result = sut.Matches(value);

			await That(result).IsTrue();
		}
	}
}
