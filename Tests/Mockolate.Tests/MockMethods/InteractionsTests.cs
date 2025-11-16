using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockMethods;

public sealed class InteractionsTests
{
	[Fact]
	public async Task Method_WhenNameAndValueMatches_ShouldReturnOnce()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;
		registration.InvokeMethod("foo.bar", 4);

		VerificationResult<IChocolateDispenser> result = registration.Method(mock, "foo.bar", new NamedParameter("p1", Any<int>()));

		await That(result).Once();
	}

	[Fact]
	public async Task Method_WhenOnlyNameMatches_ShouldReturnNever()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;
		registration.InvokeMethod("foo.bar", 4);

		VerificationResult<IChocolateDispenser> result = registration.Method(mock, "foo.bar", new NamedParameter("p1", Any<string>()));

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WhenOnlyValueMatches_ShouldReturnNever()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;
		registration.InvokeMethod("foo.bar", 4);

		VerificationResult<IChocolateDispenser> result = registration.Method(mock, "baz.bar", new NamedParameter("p1", Any<int>()));

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

		VerificationResult<IChocolateDispenser> result = registration.Method(mock, "foo.bar", new NamedParameter("p1", Any<int>()));

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("invoked method bar(Any<int>())");
	}

	[Fact]
	public async Task MethodInvocation_ToString_ShouldReturnExpectedValue()
	{
		MethodInvocation interaction = new(3, "SomeMethod", [1, null, TimeSpan.FromSeconds(90),]);
		string expectedValue = "[3] invoke method SomeMethod(1, null, 00:01:30)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
