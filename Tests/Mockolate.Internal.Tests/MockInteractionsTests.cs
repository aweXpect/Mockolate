using Mockolate.Interactions;

namespace Mockolate.Internal.Tests;

public class MockInteractionsTests
{
	[Test]
	public async Task InteractionAdded_ShouldIncludeInteraction()
	{
		int interactionCount = 0;
		MockInteractions sut = new();
		MethodInvocation interaction = new(0, "foo", []);
		sut.InteractionAdded += OnInteractionAdded;

		((IMockInteractions)sut).RegisterInteraction(interaction);

		sut.InteractionAdded -= OnInteractionAdded;

		await That(interactionCount).IsEqualTo(1);

		void OnInteractionAdded(object? sender, EventArgs e)
		{
			interactionCount++;
		}
	}

	[Test]
	public async Task RegisterInteraction_ShouldRegisterInteraction()
	{
		MockInteractions sut = new();
		MethodInvocation interaction = new(0, "foo", []);

		MethodInvocation registeredInteraction = ((IMockInteractions)sut).RegisterInteraction(interaction);

		await That(registeredInteraction).IsSameAs(interaction);
	}
}
