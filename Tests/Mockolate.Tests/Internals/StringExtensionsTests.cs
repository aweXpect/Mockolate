using Mockolate.Verify;

namespace Mockolate.Tests.Internals;

public sealed class StringExtensionsTests
{
	[Fact]
	public async Task SubstringAfterLast_WhenNameContainsNoDot_ShouldIncludeFullName()
	{
		MockRegistry registration = new(MockBehavior.Default);

		VerificationResult<int> result = registration.Property(0, "SomeNameWithoutADot");

		result.Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property SomeNameWithoutADot");
	}

	[Fact]
	public async Task SubstringAfterLast_WhenNameStartsWithDot_ShouldOmitDot()
	{
		MockRegistry registration = new(MockBehavior.Default);

		VerificationResult<int> result = registration.Property(0, ".bar");

		result.Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property bar");
	}
}
