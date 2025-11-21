using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsAnyTests
	{
		[Test]
		[Arguments(null)]
		[Arguments("")]
		[Arguments("foo")]
		public async Task ShouldAlwaysMatch(string? value)
		{
			IParameter<string> sut = It.IsAny<string>();

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsTrue();
		}

		[Test]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.IsAny<string>();
			string expectedValue = "It.IsAny<string>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
