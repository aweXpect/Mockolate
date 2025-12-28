using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsNullTests
	{
		[Test]
		[Arguments(null, 1)]
		[Arguments(1, 0)]
		public async Task Null_ShouldMatchWhenNull(int? value, int expectedCount)
		{
			IMyServiceWithNullable mock = Mock.Create<IMyServiceWithNullable>();
			mock.SetupMock.Method.DoSomething(null, It.Is(true));

			mock.DoSomething(value, true);

			await That(mock.VerifyMock.Invoked.DoSomething(null, It.Is(true))).Exactly(expectedCount);
		}

		[Test]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.IsNull<string>();
			string expectedValue = "It.IsNull<string>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Test]
		[Arguments(null, true)]
		[Arguments("", false)]
		[Arguments("foo", false)]
		public async Task WithValue_Nullable_ShouldMatchWhenEqual(string? value, bool expectMatch)
		{
			IParameter<string?> sut = It.IsNull<string?>();

			bool result = ((IParameter)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}
	}
}
