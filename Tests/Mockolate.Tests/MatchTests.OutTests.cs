namespace Mockolate.Tests;

public sealed partial class MatchTests
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
		public async Task ToString_AnyOut_ShouldReturnExpectedValue()
		{
			IOutParameter<int> sut = AnyOut<int>();
			string expectedValue = "AnyOut<int>()";

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
		[InlineData(42L, false)]
		[InlineData("foo", false)]
		[InlineData(null, true)]
		[InlineData(123, true)]
		public async Task WithAnyOut_ShouldCheckType(object? value, bool expectMatch)
		{
			IOutParameter<int?> sut = AnyOut<int?>();

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}

		[Theory]
		[InlineData(42)]
		[InlineData(-2)]
		public async Task WithOut_ShouldReturnValue(int? value)
		{
			IOutParameter<int?> sut = Out(() => value);

			int? result = sut.GetValue(MockBehavior.Default);

			await That(result).IsEqualTo(value);
		}

		[Theory]
		[InlineData(42L)]
		[InlineData("foo")]
		public async Task WithOut_Verify_ShouldAlwaysMatch(object? value)
		{
			IVerifyOutParameter<int?> sut = Out<int?>();

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsTrue();
		}
	}
}
