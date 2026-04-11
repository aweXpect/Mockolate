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
		MockRegistry registry = ((IMock)sut).MockRegistry;
		registry.RegisterInteraction(new MethodInvocation<int>("foo.bar", "p1", 4));

		VerificationResult<IChocolateDispenser> result =
			registry.Method(sut,
				new VoidMethodSetup<int>.WithParameterCollection(registry, "foo.bar",
					(IParameterMatch<int>)It.IsAny<int>()));

		await That(result).Once();
	}

	[Fact]
	public async Task Method_WhenOnlyNameMatches_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;
		registry.RegisterInteraction(new MethodInvocation<int>("foo.bar", "p1", 4));

		VerificationResult<IChocolateDispenser> result =
			registry.Method(sut,
				new VoidMethodSetup<string>.WithParameterCollection(registry, "foo.bar",
					(IParameterMatch<string>)It.IsAny<string>()));

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WhenOnlyValueMatches_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;
		registry.RegisterInteraction(new MethodInvocation<int>("foo.bar", "p1", 4));

		VerificationResult<IChocolateDispenser> result =
			registry.Method(sut,
				new VoidMethodSetup<int>.WithParameterCollection(registry, "baz.bar",
					(IParameterMatch<int>)It.IsAny<int>()));

		await That(result).Never();
	}

	[Fact]
	public async Task Method_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;

		VerificationResult<IChocolateDispenser> result =
			registry.Method(sut,
				new VoidMethodSetup<int>.WithParameterCollection(registry, "foo.bar",
					(IParameterMatch<int>)It.IsAny<int>()));

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("invoked method void bar(It.IsAny<int>())");
	}

	[Fact]
	public async Task MethodInvocation_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation<int, long?, TimeSpan> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation<int, long?, TimeSpan>("global::Mockolate.InteractionsTests.SomeMethod",
				"p1", 1,
				"p2", null,
				"p3", 90.Seconds()));
		string expectedValue = "invoke method SomeMethod(1, null, 00:01:30)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
