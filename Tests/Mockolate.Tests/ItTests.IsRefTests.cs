using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class IsRefTests
	{
		[Fact]
		public async Task WithRef_DifferentType_Long_ShouldNotMatch()
		{
			IRefParameter<int?> sut = It.IsRef<int?>(_ => true, _ => null);

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<long>(string.Empty, 42L));

			await That(result).IsFalse();
		}

		[Fact]
		public async Task WithRef_DifferentType_String_ShouldNotMatch()
		{
			IRefParameter<int?> sut = It.IsRef<int?>(_ => true, _ => null);

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<string>(string.Empty, "foo"));

			await That(result).IsFalse();
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task WithRef_ShouldMatchForExpectedResult(bool predicateValue)
		{
			IRefParameter<string> sut = It.IsRef<string>(_ => predicateValue, _ => "");

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<string>(string.Empty, "foo"));

			await That(result).IsEqualTo(predicateValue);
		}

		[Fact]
		public async Task WithRef_Verify_Long_ShouldAlwaysMatch()
		{
			IVerifyRefParameter<int?> sut = It.IsRef<int?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<long>(string.Empty, 42L));

			await That(result).IsTrue();
			await That(() => ((IParameter)sut).InvokeCallbacks(new NamedParameterValue<int>("", 0))).DoesNotThrow();
		}

		[Fact]
		public async Task WithRef_Verify_String_ShouldAlwaysMatch()
		{
			IVerifyRefParameter<int?> sut = It.IsRef<int?>();

			bool result = ((IParameter)sut).Matches(new NamedParameterValue<string>(string.Empty, "foo"));

			await That(result).IsTrue();
			await That(() => ((IParameter)sut).InvokeCallbacks(new NamedParameterValue<int>("", 0))).DoesNotThrow();
		}
	}
}
