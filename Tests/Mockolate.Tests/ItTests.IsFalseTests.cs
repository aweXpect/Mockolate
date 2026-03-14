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
			IMyServiceWithNullable mock = IMyServiceWithNullable.CreateMock();
			mock.Mock.Setup.DoSomething(null, It.IsFalse());

			mock.DoSomething(null, value);

			await That(mock.Mock.Verify.DoSomething(null, It.IsFalse())).Exactly(expectedCount);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<bool> sut = It.IsFalse();
			string expectedValue = "It.IsFalse()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
