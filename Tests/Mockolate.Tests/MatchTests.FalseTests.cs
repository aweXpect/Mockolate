using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	public sealed class FalseTests
	{
		[Theory]
		[InlineData(false, 1)]
		[InlineData(true, 0)]
		public async Task False_ShouldMatchWhenFalse(bool value, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();
			mock.SetupMock.Method.DoSomething(null, False());

			mock.DoSomething(null, value);

			await That(mock.VerifyMock.Invoked.DoSomething(null, False())).Exactly(expectedCount);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<bool> sut = False();
			string expectedValue = "False()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
