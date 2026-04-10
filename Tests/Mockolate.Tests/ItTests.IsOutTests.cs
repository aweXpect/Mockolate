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

			int? result = sut.GetValue(() => null);

			await That(result).IsEqualTo(value);
		}

		[Fact]
		public async Task WithOut_Verify_Long_ShouldAlwaysMatch()
		{
			IVerifyOutParameter<int?> sut = It.IsOut<int?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<long>(string.Empty, 42L));

			await That(result).IsTrue();
			await That(() => ((IParameter)sut).InvokeCallbacks(new NamedParameterValue<int>("", 0))).DoesNotThrow();
		}

		[Fact]
		public async Task WithOut_Verify_String_ShouldAlwaysMatch()
		{
			IVerifyOutParameter<int?> sut = It.IsOut<int?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<string>(string.Empty, "foo"));

			await That(result).IsTrue();
			await That(() => ((IParameter)sut).InvokeCallbacks(new NamedParameterValue<int>("", 0))).DoesNotThrow();
		}
	}
}
