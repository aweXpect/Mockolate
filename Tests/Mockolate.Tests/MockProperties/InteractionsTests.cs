using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockProperties;

public sealed class InteractionsTests
{
	[Fact]
	public async Task MockGot_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistration registration = ((IMock)sut).Registrations;
		registration.GetProperty("foo.bar", () => 0, null);

		VerificationResult<IChocolateDispenser> result = registration.Property(sut, "baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task MockGot_WhenNameMatches_ShouldReturnOnce()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistration registration = ((IMock)sut).Registrations;
		registration.GetProperty("foo.bar", () => 0, null);

		VerificationResult<IChocolateDispenser> result = registration.Property(sut, "foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task MockGot_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistration registration = ((IMock)sut).Registrations;

		VerificationResult<IChocolateDispenser> result = registration.Property(sut, "foo.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property bar");
	}

	[Fact]
	public async Task MockSet_WhenNameAndValueMatches_ShouldReturnOnce()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistration registration = ((IMock)sut).Registrations;
		registration.SetProperty("foo.bar", 4);

		VerificationResult<IChocolateDispenser> result = registration.Property(sut, "foo.bar", (IParameter)It.IsAny<int>());

		await That(result).Once();
	}

	[Fact]
	public async Task MockSet_WhenOnlyNameMatches_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistration registration = ((IMock)sut).Registrations;
		registration.SetProperty("foo.bar", 4);

		VerificationResult<IChocolateDispenser> result =
			registration.Property(sut, "foo.bar", (IParameter)It.IsAny<string>());

		await That(result).Never();
	}

	[Fact]
	public async Task MockSet_WhenOnlyValueMatches_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistration registration = ((IMock)sut).Registrations;
		registration.SetProperty("foo.bar", 4);

		VerificationResult<IChocolateDispenser> result = registration.Property(sut, "baz.bar", (IParameter)It.IsAny<int>());

		await That(result).Never();
	}

	[Fact]
	public async Task MockSet_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistration registration = ((IMock)sut).Registrations;

		VerificationResult<IChocolateDispenser> result = registration.Property(sut, "foo.bar", (IParameter)It.IsAny<int>());

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("set property bar to value It.IsAny<int>()");
	}

	[Fact]
	public async Task PropertyGetterAccess_ToString_ShouldReturnExpectedValue()
	{
		PropertyGetterAccess interaction = new(3, "SomeProperty");
		string expectedValue = "[3] get property SomeProperty";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task PropertySetterAccess_ToString_ShouldReturnExpectedValue()
	{
		PropertySetterAccess interaction = new(3, "SomeProperty", 5);
		string expectedValue = "[3] set property SomeProperty to 5";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task PropertySetterAccess_ToString_WithNull_ShouldReturnExpectedValue()
	{
		PropertySetterAccess interaction = new(4, "SomeProperty", null);
		string expectedValue = "[4] set property SomeProperty to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
