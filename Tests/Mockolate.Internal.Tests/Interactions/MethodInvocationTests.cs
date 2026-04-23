using Mockolate.Interactions;

namespace Mockolate.Internal.Tests.Interactions;

public sealed class MethodInvocationTests
{
	public sealed class ThreeParameterToString
	{
		[Fact]
		public async Task Should_Include_ThirdParameterValue()
		{
			MethodInvocation<int, int, int> sut = new("MyType.MyMethod", "a", 1, "b", 2, "c", 3);

			string result = sut.ToString();

			await That(result).IsEqualTo("invoke method MyMethod(1, 2, 3)");
		}

		[Fact]
		public async Task WithNullThirdParameter_Should_FormatAsNullLiteral()
		{
			MethodInvocation<string?, string?, string?> sut =
				new("MyType.MyMethod", "a", null, "b", null, "c", null);

			string result = sut.ToString();

			await That(result).IsEqualTo("invoke method MyMethod(null, null, null)");
		}
	}
}
