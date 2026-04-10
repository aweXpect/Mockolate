using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsNotNullTests
	{
		[Theory]
		[InlineData(null, 0)]
		[InlineData(1, 1)]
		public async Task NotNull_ShouldMatchWhenNotNull(int? value, int expectedCount)
		{
			IMyServiceWithNullable sut = IMyServiceWithNullable.CreateMock();
			sut.Mock.Setup.DoSomething(It.IsNotNull<int?>(), It.Is(true));

			sut.DoSomething(value, true);

			await That(sut.Mock.Verify.DoSomething(It.IsNotNull<int?>(), It.Is(true))).Exactly(expectedCount);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.IsNotNull<string>();
			string expectedValue = "It.IsNotNull<string>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_WithCustomValue_ShouldReturnCustomValue()
		{
			IParameter<string> sut = It.IsNotNull<string>("custom");
			string expectedValue = "custom";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Theory]
		[InlineData(null, false)]
		[InlineData("", true)]
		[InlineData("foo", true)]
		public async Task WithValue_Nullable_ShouldMatchWhenNotNull(string? value, bool expectMatch)
		{
			IParameter<string?> sut = It.IsNotNull<string?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<string?>(string.Empty, value));

			await That(result).IsEqualTo(expectMatch);
		}
	}
}
