using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockProperties;

public sealed class InteractionsTests
{
	[Test]
	public async Task MockGot_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;
		registration.GetProperty("foo.bar", () => 0, null);

		VerificationResult<IChocolateDispenser> result = registration.Property(mock, "baz.bar");

		await That(result).Never();
	}

	[Test]
	public async Task MockGot_WhenNameMatches_ShouldReturnOnce()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;
		registration.GetProperty("foo.bar", () => 0, null);

		VerificationResult<IChocolateDispenser> result = registration.Property(mock, "foo.bar");

		await That(result).Once();
	}

	[Test]
	public async Task MockGot_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

		VerificationResult<IChocolateDispenser> result = registration.Property(mock, "foo.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property bar");
	}

	[Test]
	public async Task MockSet_WhenNameAndValueMatches_ShouldReturnOnce()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;
		registration.SetProperty("foo.bar", 4);

		VerificationResult<IChocolateDispenser> result = registration.Property(mock, "foo.bar", (IParameter)It.IsAny<int>());

		await That(result).Once();
	}

	[Test]
	public async Task MockSet_WhenOnlyNameMatches_ShouldReturnNever()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;
		registration.SetProperty("foo.bar", 4);

		VerificationResult<IChocolateDispenser> result =
			registration.Property(mock, "foo.bar", (IParameter)It.IsAny<string>());

		await That(result).Never();
	}

	[Test]
	public async Task MockSet_WhenOnlyValueMatches_ShouldReturnNever()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;
		registration.SetProperty("foo.bar", 4);

		VerificationResult<IChocolateDispenser> result = registration.Property(mock, "baz.bar", (IParameter)It.IsAny<int>());

		await That(result).Never();
	}

	[Test]
	public async Task MockSet_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

		VerificationResult<IChocolateDispenser> result = registration.Property(mock, "foo.bar", (IParameter)It.IsAny<int>());

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("set property bar to value It.IsAny<int>()");
	}

	[Test]
	public async Task PropertyGetterAccess_ToString_ShouldReturnExpectedValue()
	{
		PropertyGetterAccess interaction = new(3, "SomeProperty");
		string expectedValue = "[3] get property SomeProperty";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Test]
	public async Task PropertySetterAccess_ToString_ShouldReturnExpectedValue()
	{
		PropertySetterAccess interaction = new(3, "SomeProperty", 5);
		string expectedValue = "[3] set property SomeProperty to 5";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Test]
	public async Task PropertySetterAccess_ToString_WithNull_ShouldReturnExpectedValue()
	{
		PropertySetterAccess interaction = new(4, "SomeProperty", null);
		string expectedValue = "[4] set property SomeProperty to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
