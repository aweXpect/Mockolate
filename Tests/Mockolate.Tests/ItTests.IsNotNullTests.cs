using Mockolate.Parameters;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsNotNullTests
	{
		[Fact]
		public async Task CachedMatcher_CallbackFromPriorDo_ShouldNotLeakIntoSubsequentUsage()
		{
			_ = It.IsNotNull<string>().Do(_ => throw new InvalidOperationException("callback leaked from prior use"));

			IParameter<string> subsequent = It.IsNotNull<string>();

			await That(() => ((IParameterMatch<string>)subsequent).InvokeCallbacks("value")).DoesNotThrow();
		}

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
		public async Task ShouldReturnSharedInstance_WhenToStringIsOmitted()
		{
			IParameter<string> first = It.IsNotNull<string>();
			IParameter<string> second = It.IsNotNull<string>();

			await That(first).IsSameAs(second);
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

		[Fact]
		public async Task WhenTypeDoesNotMatch_ShouldReturnTrue()
		{
			MyFlavor flavor = MyFlavor.Dark;
			IParameter<string?> sut = It.IsNotNull<string?>();

			bool result = sut.Matches(flavor);

			await That(result).IsTrue();
		}

		[Theory]
		[InlineData(null, false)]
		[InlineData("", true)]
		[InlineData("foo", true)]
		public async Task WithValue_Nullable_ShouldMatchWhenNotNull(string? value, bool expectMatch)
		{
			IParameter<string?> sut = It.IsNotNull<string?>();

			bool result = ((IParameterMatch<string?>)sut).Matches(value);

			await That(result).IsEqualTo(expectMatch);
		}
	}
}
