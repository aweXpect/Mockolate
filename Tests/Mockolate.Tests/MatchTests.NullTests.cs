namespace Mockolate.Tests;

public sealed partial class MatchTests
{
	public sealed class NullTests
	{
		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = Null<string>();
			string expectedValue = "Null<string>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(null, 1)]
		[InlineData(1, 0)]
		public async Task WithNull_ShouldMatchWhenNull(int? value, int expectedCount)
		{
			Mock<IMyServiceWithNullable> mock = Mock.Create<IMyServiceWithNullable>();
			mock.Setup.Method.DoSomething(null, With(true));

			mock.Subject.DoSomething(value, true);

			await That(mock.Verify.Invoked.DoSomething(null, With(true))).Exactly(expectedCount);
		}
	}
}
