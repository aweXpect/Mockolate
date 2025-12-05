using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	public sealed class NullTests
	{
		[Theory]
		[InlineData(null, 1)]
		[InlineData(1, 0)]
		public async Task Null_ShouldMatchWhenNull(int? value, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();
			mock.SetupMock.Method.DoSomething(null, With(true));

			mock.DoSomething(value, true);

			await That(mock.VerifyMock.Invoked.DoSomething(null, With(true))).Exactly(expectedCount);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = Null<string>();
			string expectedValue = "Null<string>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
