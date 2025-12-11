using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsAnyParametersTests
	{
		[Theory]
		[InlineData(null, null)]
		[InlineData("", 1)]
		[InlineData("foo", null)]
		public async Task ShouldAlwaysMatch(params object?[] values)
		{
			IParameters sut = Match.AnyParameters();

			bool result = sut.Matches(values);

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameters sut = Match.AnyParameters();
			string expectedValue = "AnyParameters()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
