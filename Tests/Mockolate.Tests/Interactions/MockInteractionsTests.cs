using System.Collections.Generic;
using Mockolate.Interactions;

namespace Mockolate.Tests.Interactions;

public class MockInteractionsTests
{
	[Fact]
	public async Task InteractionAdded_ShouldIncludeInteraction()
	{
		List<IInteraction> addedInteractions = [];
		MockInteractions sut = new();
		MethodInvocation interaction = new(0, "foo", []);
		sut.InteractionAdded += OnInteractionAdded;

		((IMockInteractions)sut).RegisterInteraction(interaction);

		sut.InteractionAdded -= OnInteractionAdded;

		await That(addedInteractions).HasSingle().Which.IsSameAs(interaction);

		void OnInteractionAdded(object? sender, IInteraction e)
		{
			addedInteractions.Add(e);
		}
	}

	[Fact]
	public async Task RegisterInteraction_ShouldRegisterInteraction()
	{
		MockInteractions sut = new();
		MethodInvocation interaction = new(0, "foo", []);

		MethodInvocation registeredInteraction = ((IMockInteractions)sut).RegisterInteraction(interaction);

		await That(registeredInteraction).IsSameAs(interaction);
	}
}
