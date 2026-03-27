using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsTrueTests
	{
		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			var sut =It.IsTrue();
			string expectedValue = "It.IsTrue()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(true, 1)]
		[InlineData(false, 0)]
		public async Task True_ShouldMatchWhenTrue(bool value, int expectedCount)
		{
			IMyServiceWithNullable sut = IMyServiceWithNullable.CreateMock();
			sut.Mock.Setup.DoSomething(null, It.IsTrue());

			sut.DoSomething(null, value);

			await That(sut.Mock.Verify.DoSomething(null, It.IsTrue())).Exactly(expectedCount);
		}
	}
}
