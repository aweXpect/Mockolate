using Mockolate.Parameters;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsNullTests
	{
		[Fact]
		public async Task CachedMatcher_CallbackFromPriorDo_ShouldNotLeakIntoSubsequentUsage()
		{
			_ = It.IsNull<string?>().Do(_ => throw new InvalidOperationException("callback leaked from prior use"));

			IParameter<string?> subsequent = It.IsNull<string?>();

			await That(() => ((IParameterMatch<string?>)subsequent).InvokeCallbacks(null)).DoesNotThrow();
		}

		[Theory]
		[InlineData(null, 1)]
		[InlineData(1, 0)]
		public async Task Null_ShouldMatchWhenNull(int? value, int expectedCount)
		{
			IMyServiceWithNullable sut = IMyServiceWithNullable.CreateMock();
			sut.Mock.Setup.DoSomething(null, It.Is(true));

			sut.DoSomething(value, true);

			await That(sut.Mock.Verify.DoSomething(null, It.Is(true))).Exactly(expectedCount);
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<string> sut = It.IsNull<string>();
			string expectedValue = "It.IsNull<string>()";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task WhenTypeDoesNotMatch_ShouldReturnFalse()
		{
			MyFlavor flavor = MyFlavor.Dark;
			IParameter<string?> sut = It.IsNull<string?>();

			bool result = sut.Matches(flavor);

			await That(result).IsFalse();
		}

		[Theory]
		[InlineData(null, true)]
		[InlineData("", false)]
		[InlineData("foo", false)]
		public async Task WithValue_Nullable_ShouldMatchWhenEqual(string? value, bool expectMatch)
		{
			IParameter<string?> sut = It.IsNull<string?>();

			bool result = ((IParameterMatch<string?>)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}
	}
}
