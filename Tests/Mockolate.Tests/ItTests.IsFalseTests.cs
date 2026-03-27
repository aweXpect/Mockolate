using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsFalseTests
	{
		[Theory]
		[InlineData(false, 1)]
		[InlineData(true, 0)]
		public async Task False_ShouldMatchWhenFalse(bool value, int expectedCount)
		{
			IMyServiceWithNullable sut = IMyServiceWithNullable.CreateMock();
			sut.Mock.Setup.DoSomething(null, It.IsFalse());

			sut.DoSomething(null, value);

			await That(sut.Mock.Verify.DoSomething(null, It.IsFalse())).Exactly(expectedCount);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			var sut =It.IsFalse();
			string expectedValue = "It.IsFalse()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
