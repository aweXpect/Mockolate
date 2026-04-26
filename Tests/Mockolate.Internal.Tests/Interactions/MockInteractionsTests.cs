using System.Collections.Generic;
using System.Linq;
using Mockolate.Interactions;

namespace Mockolate.Internal.Tests.Interactions;

public class MockInteractionsTests
{
	[Fact]
	public async Task InteractionAdded_ShouldIncludeInteraction()
	{
		int interactionCount = 0;
		MockInteractions sut = new();
		MethodInvocation interaction = new("foo");
		sut.InteractionAdded += OnInteractionAdded;

		((IMockInteractions)sut).RegisterInteraction(interaction);

		sut.InteractionAdded -= OnInteractionAdded;

		await That(interactionCount).IsEqualTo(1);

		void OnInteractionAdded(object? sender, EventArgs e)
		{
			interactionCount++;
		}
	}

	[Fact]
	public async Task RegisterInteraction_ShouldRegisterInteraction()
	{
		MockInteractions sut = new();
		MethodInvocation interaction = new("foo");

		MethodInvocation registeredInteraction = ((IMockInteractions)sut).RegisterInteraction(interaction);

		await That(registeredInteraction).IsSameAs(interaction);
	}

	public sealed class InterfaceSurface
	{
		[Fact]
		public async Task Count_ShouldReflectRegisteredInteractions()
		{
			IMockInteractions sut = new MockInteractions();
			sut.RegisterInteraction(new MethodInvocation("first"));
			sut.RegisterInteraction(new MethodInvocation("second"));

			await That(sut.Count).IsEqualTo(2);
		}

		[Fact]
		public async Task GetEnumerator_ShouldYieldRegisteredInteractionsInOrder()
		{
			IMockInteractions sut = new MockInteractions();
			MethodInvocation first = new("first");
			MethodInvocation second = new("second");
			sut.RegisterInteraction(first);
			sut.RegisterInteraction(second);

			List<IInteraction> enumerated = [.. sut];

			await That(enumerated).HasCount(2);
			await That(enumerated[0]).IsSameAs(first);
			await That(enumerated[1]).IsSameAs(second);
		}

		[Fact]
		public async Task SkipInteractionRecording_ShouldReflectConstructionValue()
		{
			MockInteractions skipping = new MockInteractions { SkipInteractionRecording = true };
			MockInteractions recording = new MockInteractions { SkipInteractionRecording = false };

			await That(skipping.SkipInteractionRecording).IsTrue();
			await That(recording.SkipInteractionRecording).IsFalse();
		}

		[Fact]
		public async Task SkipInteractionRecording_WhenTrue_ShouldSuppressRegister()
		{
			IMockInteractions sut = new MockInteractions { SkipInteractionRecording = true };

			sut.RegisterInteraction(new MethodInvocation("foo"));

			await That(sut.Count).IsEqualTo(0);
		}

		[Fact]
		public async Task InteractionAdded_ShouldFireWhenInteractionIsRegistered()
		{
			int invocations = 0;
			IMockInteractions sut = new MockInteractions();
			sut.InteractionAdded += Handler;

			sut.RegisterInteraction(new MethodInvocation("foo"));

			sut.InteractionAdded -= Handler;

			await That(invocations).IsEqualTo(1);

			void Handler(object? sender, EventArgs e) => invocations++;
		}

		[Fact]
		public async Task OnClearing_ShouldFireWhenClearIsInvoked()
		{
			int invocations = 0;
			IMockInteractions sut = new MockInteractions();
			sut.RegisterInteraction(new MethodInvocation("foo"));
			sut.OnClearing += Handler;

			sut.Clear();

			sut.OnClearing -= Handler;

			await That(invocations).IsEqualTo(1);

			void Handler(object? sender, EventArgs e) => invocations++;
		}

		[Fact]
		public async Task Clear_ShouldEmptyRecordedInteractions()
		{
			IMockInteractions sut = new MockInteractions();
			sut.RegisterInteraction(new MethodInvocation("first"));
			sut.RegisterInteraction(new MethodInvocation("second"));

			sut.Clear();

			await That(sut.Count).IsEqualTo(0);
		}

		[Fact]
		public async Task GetUnverifiedInteractions_ShouldReturnEverythingWhenNothingVerified()
		{
			IMockInteractions sut = new MockInteractions();
			MethodInvocation first = new("first");
			MethodInvocation second = new("second");
			sut.RegisterInteraction(first);
			sut.RegisterInteraction(second);

			IReadOnlyCollection<IInteraction> unverified = sut.GetUnverifiedInteractions();

			await That(unverified).HasCount(2);
			await That(unverified.Contains(first)).IsTrue();
			await That(unverified.Contains(second)).IsTrue();
		}

		[Fact]
		public async Task Verified_ShouldRemoveMarkedInteractionsFromGetUnverifiedInteractions()
		{
			IMockInteractions sut = new MockInteractions();
			MethodInvocation first = new("first");
			MethodInvocation second = new("second");
			sut.RegisterInteraction(first);
			sut.RegisterInteraction(second);

			sut.Verified([first]);

			IReadOnlyCollection<IInteraction> unverified = sut.GetUnverifiedInteractions();

			await That(unverified).HasCount(1);
			await That(unverified.Contains(first)).IsFalse();
			await That(unverified.Contains(second)).IsTrue();
		}

		[Fact]
		public async Task Clear_ShouldResetVerifiedBookkeeping()
		{
			IMockInteractions sut = new MockInteractions();
			MethodInvocation first = new("first");
			sut.RegisterInteraction(first);
			sut.Verified([first]);

			sut.Clear();

			MethodInvocation second = new("second");
			sut.RegisterInteraction(second);

			IReadOnlyCollection<IInteraction> unverified = sut.GetUnverifiedInteractions();

			await That(unverified).HasCount(1);
			await That(unverified.Contains(second)).IsTrue();
		}
	}
}
