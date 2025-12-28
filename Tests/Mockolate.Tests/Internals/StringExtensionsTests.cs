using Mockolate.Verify;

namespace Mockolate.Tests.Internals;

public sealed class StringExtensionsTests
{
	[Test]
	public async Task SubstringAfterLast_WhenNameContainsNoDot_ShouldIncludeFullName()
	{
		MockRegistration registration = new(MockBehavior.Default, "");

		VerificationResult<int> result = registration.Property(0, "SomeNameWithoutADot");

		result.Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property SomeNameWithoutADot");
	}

	[Test]
	public async Task SubstringAfterLast_WhenNameStartsWithDot_ShouldOmitDot()
	{
		MockRegistration registration = new(MockBehavior.Default, "");

		VerificationResult<int> result = registration.Property(0, ".bar");

		result.Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property bar");
	}
}
