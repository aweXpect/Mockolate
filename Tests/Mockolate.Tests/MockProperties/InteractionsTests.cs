using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockProperties;

public sealed partial class InteractionsTests
{
	[Fact]
	public async Task MockGot_WhenNameDoesNotMatch_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;
		registry.GetProperty("foo.bar", () => 0, null);

		VerificationResult<IChocolateDispenser> result = registry.VerifyProperty(sut, "baz.bar");

		await That(result).Never();
	}

	[Fact]
	public async Task MockGot_WhenNameMatches_ShouldReturnOnce()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;
		registry.GetProperty("foo.bar", () => 0, null);

		VerificationResult<IChocolateDispenser> result = registry.VerifyProperty(sut, "foo.bar");

		await That(result).Once();
	}

	[Fact]
	public async Task MockGot_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;

		VerificationResult<IChocolateDispenser> result = registry.VerifyProperty(sut, "foo.bar");

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("got property bar");
	}

	[Fact]
	public async Task MockSet_WhenNameAndValueMatches_ShouldReturnOnce()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;
		registry.SetProperty("foo.bar", 4);

		VerificationResult<IChocolateDispenser> result = registry.VerifyProperty(sut, "foo.bar", (IParameterMatch<int>)It.IsAny<int>());

		await That(result).Once();
	}

	[Fact]
	public async Task MockSet_WhenOnlyNameMatches_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;
		registry.SetProperty("foo.bar", 4);

		VerificationResult<IChocolateDispenser> result =
			registry.VerifyProperty(sut, "foo.bar", (IParameterMatch<string>)It.IsAny<string>());

		await That(result).Never();
	}

	[Fact]
	public async Task MockSet_WhenOnlyValueMatches_ShouldReturnNever()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;
		registry.SetProperty("foo.bar", 4);

		VerificationResult<IChocolateDispenser> result = registry.VerifyProperty(sut, "baz.bar", (IParameterMatch<int>)It.IsAny<int>());

		await That(result).Never();
	}

	[Fact]
	public async Task MockSet_WithoutInteractions_ShouldReturnNeverResult()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockRegistry registry = ((IMock)sut).MockRegistry;

		VerificationResult<IChocolateDispenser> result = registry.VerifyProperty(sut, "foo.bar", (IParameterMatch<int>)It.IsAny<int>());

		await That(result).Never();
		await That(((IVerificationResult)result).Expectation).IsEqualTo("set property bar to It.IsAny<int>()");
	}

	[Fact]
	public async Task PropertyGetterAccess_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		PropertyGetterAccess interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new PropertyGetterAccess("global::Mockolate.InteractionsTests.SomeProperty"));
		string expectedValue = "get property SomeProperty";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task PropertySetterAccess_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		PropertySetterAccess<int> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new PropertySetterAccess<int>("global::Mockolate.InteractionsTests.SomeProperty", 5));
		string expectedValue = "set property SomeProperty to 5";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task PropertySetterAccess_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		PropertySetterAccess<string?> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new PropertySetterAccess<string?>("SomeProperty", null));
		string expectedValue = "set property SomeProperty to null";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
