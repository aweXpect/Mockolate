using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsFalseTests
	{
		[Test]
		[Arguments(false, 1)]
		[Arguments(true, 0)]
		public async Task False_ShouldMatchWhenFalse(bool value, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();
			mock.SetupMock.Method.DoSomething(null, It.IsFalse());

			mock.DoSomething(null, value);

			await That(mock.VerifyMock.Invoked.DoSomething(null, It.IsFalse())).Exactly(expectedCount);
		}

		[Test]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<bool> sut = It.IsFalse();
			string expectedValue = "It.IsFalse()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}
	}
}
