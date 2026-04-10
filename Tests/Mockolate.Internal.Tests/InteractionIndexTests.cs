using System.Reflection;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Internal.Tests;

public sealed class InteractionIndexTests
{
	[Fact]
	public async Task EventSubscription_SetIndexTwice_ShouldRemainUnchanged()
	{
		EventSubscription interaction = new("SomeEvent", this, GetMethodInfo());
		((ISettableInteraction)interaction).SetIndex(1);

		((ISettableInteraction)interaction).SetIndex(2);

		await That(interaction.Index).IsEqualTo(1);
	}

	[Fact]
	public async Task EventUnsubscription_SetIndexTwice_ShouldRemainUnchanged()
	{
		EventUnsubscription interaction = new("SomeEvent", this, GetMethodInfo());
		((ISettableInteraction)interaction).SetIndex(1);

		((ISettableInteraction)interaction).SetIndex(2);

		await That(interaction.Index).IsEqualTo(1);
	}

	[Fact]
	public async Task IndexerGetterAccess_SetIndexTwice_ShouldRemainUnchanged()
	{
		IndexerGetterAccess interaction = new([new NamedParameterValue<string>("p1", "SomeProperty"),]);
		((ISettableInteraction)interaction).SetIndex(1);

		((ISettableInteraction)interaction).SetIndex(2);

		await That(interaction.Index).IsEqualTo(1);
	}

	[Fact]
	public async Task IndexerSetterAccess_SetIndexTwice_ShouldRemainUnchanged()
	{
		IndexerSetterAccess interaction = new([new NamedParameterValue<string>("p1", "SomeProperty"),], new NamedParameterValue<string>("value", "foo"));
		((ISettableInteraction)interaction).SetIndex(1);

		((ISettableInteraction)interaction).SetIndex(2);

		await That(interaction.Index).IsEqualTo(1);
	}

	[Fact]
	public async Task MethodInvocation_SetIndexTwice_ShouldRemainUnchanged()
	{
		MethodInvocation interaction = new("SomeMethod", []);
		((ISettableInteraction)interaction).SetIndex(1);

		((ISettableInteraction)interaction).SetIndex(2);

		await That(interaction.Index).IsEqualTo(1);
	}

	private static MethodInfo GetMethodInfo()
		=> typeof(InteractionIndexTests).GetMethod(nameof(GetMethodInfo), BindingFlags.Static | BindingFlags.NonPublic)!;
}
