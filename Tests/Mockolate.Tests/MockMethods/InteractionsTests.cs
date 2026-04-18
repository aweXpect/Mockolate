using aweXpect.Chronology;
using Mockolate.Interactions;

namespace Mockolate.Tests.MockMethods;

public sealed partial class InteractionsTests
{
	[Fact]
	public async Task MethodInvocation_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation("global::Mockolate.InteractionsTests.SomeMethod"));
		string expectedValue = "invoke method SomeMethod()";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task MethodInvocation1_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation<int> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation<int>(
				"global::Mockolate.InteractionsTests.SomeMethod",
				"p1", 5));
		string expectedValue = "invoke method SomeMethod(5)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task MethodInvocation1_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation<string?> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation<string?>(
				"SomeMethod",
				"p1", null));
		string expectedValue = "invoke method SomeMethod(null)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task MethodInvocation2_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation<int, string> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation<int, string>(
				"global::Mockolate.InteractionsTests.SomeMethod",
				"p1", 5,
				"p2", "foo"));
		string expectedValue = "invoke method SomeMethod(5, foo)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task MethodInvocation2_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation<string?, long?> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation<string?, long?>(
				"SomeMethod",
				"p1", null,
				"p2", null));
		string expectedValue = "invoke method SomeMethod(null, null)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task MethodInvocation3_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation<int, long?, TimeSpan> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation<int, long?, TimeSpan>(
				"global::Mockolate.InteractionsTests.SomeMethod",
				"p1", 1,
				"p2", null,
				"p3", 90.Seconds()));
		string expectedValue = "invoke method SomeMethod(1, null, 00:01:30)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task MethodInvocation3_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation<int?, long?, TimeSpan?> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation<int?, long?, TimeSpan?>(
				"global::Mockolate.InteractionsTests.SomeMethod",
				"p1", null,
				"p2", null,
				"p3", null));
		string expectedValue = "invoke method SomeMethod(null, null, null)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task MethodInvocation4_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation<string, int, long?, TimeSpan> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation<string, int, long?, TimeSpan>(
				"global::Mockolate.InteractionsTests.SomeMethod",
				"p1", "foo",
				"p2", 4,
				"p3", 7L,
				"p4", 150.Seconds()));
		string expectedValue = "invoke method SomeMethod(foo, 4, 7, 00:02:30)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task MethodInvocation4_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation<string?, int?, long?, TimeSpan?> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation<string?, int?, long?, TimeSpan?>(
				"SomeMethod",
				"p1", null,
				"p2", null,
				"p3", null,
				"p4", null));
		string expectedValue = "invoke method SomeMethod(null, null, null, null)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task MethodInvocation5_ToString_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation<string, int, long?, TimeSpan, bool> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation<string, int, long?, TimeSpan, bool>(
				"global::Mockolate.InteractionsTests.SomeMethod",
				"p1", "foo",
				"p2", 4,
				"p3", 7L,
				"p4", 150.Seconds(),
				"p5", true));
		string expectedValue = "invoke method SomeMethod(foo, 4, 7, 00:02:30, True)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}

	[Fact]
	public async Task MethodInvocation5_ToString_WithNull_ShouldReturnExpectedValue()
	{
		MockInteractions interactions = new();
		MethodInvocation<string?, int?, long?, TimeSpan?, bool?> interaction = ((IMockInteractions)interactions).RegisterInteraction(
			new MethodInvocation<string?, int?, long?, TimeSpan?, bool?>(
				"SomeMethod",
				"p1", null,
				"p2", null,
				"p3", null,
				"p4", null,
				"p5", null));
		string expectedValue = "invoke method SomeMethod(null, null, null, null, null)";

		await That(interaction.ToString()).IsEqualTo(expectedValue);
	}
}
