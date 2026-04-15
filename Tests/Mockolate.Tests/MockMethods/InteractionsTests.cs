using aweXpect.Chronology;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockMethods;

public sealed partial class InteractionsTests
{
	[Fact]
	public async Task Method_WhenNameAndValueMatches_ShouldReturnOnce()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registration = ((IMock)sut).MockRegistry;
		registration.InvokeMethod("foo.bar", new NamedParameterValue<int>("p1", 4));

		VerificationResult<IChocolateDispenser> result =
			registration.Method(sut,
				new MethodParameterMatch("foo.bar", [new NamedParameter("p1", (IParameter)It.IsAny<int>()),]));

		await That(result).Once();
	}

	[Fact]
	public async Task Method_WhenOnlyNameMatches_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registration = ((IMock)sut).MockRegistry;
		registration.InvokeMethod("foo.bar", new NamedParameterValue<int>("p1", 4));

		VerificationResult<IChocolateDispenser> result =
			registration.Method(sut,
				new MethodParameterMatch("foo.bar", [new NamedParameter("p1", (IParameter)It.IsAny<string>()),]));

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WhenOnlyValueMatches_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registration = ((IMock)sut).MockRegistry;
		registration.InvokeMethod("foo.bar", new NamedParameterValue<int>("p1", 4));

		VerificationResult<IChocolateDispenser> result =
			registration.Method(sut,
				new MethodParameterMatch("baz.bar", [new NamedParameter("p1", (IParameter)It.IsAny<int>()),]));

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registration = ((IMock)sut).MockRegistry;

		VerificationResult<IChocolateDispenser> result =
			registration.Method(sut,
				new MethodParameterMatch("foo.bar", [new NamedParameter("p1", (IParameter)It.IsAny<int>()),]));

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("invoked method bar(It.IsAny<int>())");
	}

	[Fact]
	public async Task MethodInvocation_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation("global::Mockolate.InteractionsTests.SomeMethod", [
				new NamedParameterValue<int>("p1", 1),
				new NamedParameterValue<long?>("p2", null),
				new NamedParameterValue<TimeSpan>("p3", 90.Seconds()),
			]));
		string expectedValue = "invoke method SomeMethod(1, null, 00:01:30)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
