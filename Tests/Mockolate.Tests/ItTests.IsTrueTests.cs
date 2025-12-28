using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsTrueTests
	{
		[Test]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<bool> sut = It.IsTrue();
			string expectedValue = "It.IsTrue()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Test]
		[Arguments(true, 1)]
		[Arguments(false, 0)]
		public async Task True_ShouldMatchWhenTrue(bool value, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();
			mock.SetupMock.Method.DoSomething(null, It.IsTrue());

			mock.DoSomething(null, value);

			await That(mock.VerifyMock.Invoked.DoSomething(null, It.IsTrue())).Exactly(expectedCount);
		}
	}
}
