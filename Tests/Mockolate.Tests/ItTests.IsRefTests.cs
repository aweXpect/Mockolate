using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsRefTests
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task WithRef_ShouldMatchForExpectedResult(bool predicateValue)
		{
			IRefParameter<string> sut = It.IsRef<string>(_ => predicateValue, _ => "");

			bool result = ((IParameterMatch<string>)sut).Matches("foo");

			await That(result).IsEqualTo(predicateValue);
		}

		[Fact]
		public async Task WithRef_Verify_ShouldAlwaysMatch()
		{
			IVerifyRefParameter<int?> sut = It.IsRef<int?>();

			bool result = ((IParameterMatch<int?>)sut).Matches(42);

			await That(result).IsTrue();
			await That(() => ((IParameterMatch<int?>)sut).InvokeCallbacks(0)).DoesNotThrow();
		}
	}
}
