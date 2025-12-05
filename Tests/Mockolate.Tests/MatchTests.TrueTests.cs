using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	public sealed class TrueTests
	{
		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<bool> sut = True();
			string expectedValue = "True()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(true, 1)]
		[InlineData(false, 0)]
		public async Task True_ShouldMatchWhenTrue(bool value, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();
			mock.SetupMock.Method.DoSomething(null, True());

			mock.DoSomething(null, value);

			await That(mock.VerifyMock.Invoked.DoSomething(null, True())).Exactly(expectedCount);
		}
	}
}
