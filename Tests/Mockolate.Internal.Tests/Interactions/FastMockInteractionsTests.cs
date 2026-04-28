using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Internal.Tests.Interactions;

public class FastMockInteractionsTests
{
	[Fact]
	public async Task Append_ShouldRaiseInteractionAdded()
	{
		FastMockInteractions sut = new(1);
		FastMethod0Buffer buffer = sut.InstallMethod(0);

		int invocations = 0;
		sut.InteractionAdded += Handler;

		buffer.Append("foo");
		buffer.Append("bar");

		sut.InteractionAdded -= Handler;

		await That(invocations).IsEqualTo(2);

		void Handler(object? sender, EventArgs e)
		{
			invocations++;
		}
	}

	[Fact]
	public async Task Buffers_ShouldExposeInstalledBuffersByMemberId()
	{
		FastMockInteractions sut = new(3);
		FastMethod0Buffer method = sut.InstallMethod(0);
		FastPropertyGetterBuffer getter = sut.InstallPropertyGetter(1);
		FastEventBuffer subscribe = sut.InstallEventSubscribe(2);

		await That(sut.Buffers).HasCount(3);
		await That(sut.Buffers[0]).IsSameAs(method);
		await That(sut.Buffers[1]).IsSameAs(getter);
		await That(sut.Buffers[2]).IsSameAs(subscribe);
	}

	[Fact]
	public async Task Clear_FallbackPath_ShouldNotResurfaceOldRecordsAfterRefill()
	{
		IMockInteractions sut = new FastMockInteractions(0);
		MethodInvocation first = new("first");
		MethodInvocation second = new("second");

		sut.RegisterInteraction(first);
		sut.RegisterInteraction(second);

		sut.Clear();

		MethodInvocation third = new("third");
		MethodInvocation fourth = new("fourth");
		sut.RegisterInteraction(third);
		sut.RegisterInteraction(fourth);

		List<IInteraction> ordered = [..sut,];

		await That(sut.Count).IsEqualTo(2);
		await That(ordered).HasCount(2);
		await That(ordered.Contains(first)).IsFalse();
		await That(ordered.Contains(second)).IsFalse();
		await That(ordered[0]).IsSameAs(third);
		await That(ordered[1]).IsSameAs(fourth);
	}

	[Fact]
	public async Task Clear_ShouldFireOnClearing()
	{
		FastMockInteractions sut = new(1);
		sut.InstallMethod(0);
		int invocations = 0;
		sut.OnClearing += Handler;

		sut.Clear();

		sut.OnClearing -= Handler;

		await That(invocations).IsEqualTo(1);

		void Handler(object? sender, EventArgs e)
		{
			invocations++;
		}
	}

	[Fact]
	public async Task Clear_ShouldResetAllBuffersAndCount()
	{
		FastMockInteractions sut = new(2);
		FastMethod0Buffer methodBuffer = sut.InstallMethod(0);
		FastPropertyGetterBuffer getterBuffer = sut.InstallPropertyGetter(1);

		methodBuffer.Append("a");
		getterBuffer.Append("b");

		sut.Clear();

		await That(sut.Count).IsEqualTo(0);
		await That(methodBuffer.Count).IsEqualTo(0);
		await That(getterBuffer.Count).IsEqualTo(0);
		await That(sut.ToList()).IsEmpty();
	}

	[Fact]
	public async Task Clear_ShouldResetPerBufferVerifiedTracking()
	{
		FastMockInteractions sut = new(2);
		FastMethod1Buffer<int> methodBuffer = sut.InstallMethod<int>(0);
		FastPropertyGetterBuffer getterBuffer = sut.InstallPropertyGetter(1);

		methodBuffer.Append("M", 1);
		getterBuffer.Append("Prop");
		_ = methodBuffer.ConsumeMatching((IParameterMatch<int>)It.Is(1));
		_ = getterBuffer.ConsumeMatching();

		sut.Clear();

		methodBuffer.Append("M", 2);
		getterBuffer.Append("Prop");

		await That(sut.GetUnverifiedInteractions()).HasCount(2);
	}

	[Fact]
	public async Task Clear_ShouldResetVerifiedBookkeeping()
	{
		FastMockInteractions sut = new(1);
		FastMethod0Buffer methodBuffer = sut.InstallMethod(0);
		methodBuffer.Append("first");
		List<IInteraction> all = [..sut,];
		((IMockInteractions)sut).Verified(all);

		sut.Clear();
		methodBuffer.Append("second");

		IReadOnlyCollection<IInteraction> unverified = sut.GetUnverifiedInteractions();

		await That(unverified).HasCount(1);
	}

	[Fact]
	public async Task Count_ShouldReflectAppendsAcrossBuffers()
	{
		FastMockInteractions sut = new(2);
		FastMethod0Buffer methodBuffer = sut.InstallMethod(0);
		FastPropertyGetterBuffer getterBuffer = sut.InstallPropertyGetter(1);

		methodBuffer.Append("a");
		getterBuffer.Append("b");
		methodBuffer.Append("c");

		await That(sut.Count).IsEqualTo(3);
		await That(methodBuffer.Count).IsEqualTo(2);
		await That(getterBuffer.Count).IsEqualTo(1);
	}

	[Fact]
	public async Task GetEnumerator_ShouldReturnInteractionsInRegistrationOrder()
	{
		FastMockInteractions sut = new(2);
		FastMethod1Buffer<int> methodBuffer = sut.InstallMethod<int>(0);
		FastPropertySetterBuffer<string> setterBuffer = sut.InstallPropertySetter<string>(1);

		methodBuffer.Append("Method", 1);
		setterBuffer.Append("Property", "x");
		methodBuffer.Append("Method", 2);
		setterBuffer.Append("Property", "y");

		List<IInteraction> ordered = [..sut,];

		await That(ordered).HasCount(4);
		await That(ordered[0] is MethodInvocation<int>).IsTrue();
		await That(ordered[1] is PropertySetterAccess<string>).IsTrue();
		await That(ordered[2] is MethodInvocation<int>).IsTrue();
		await That(ordered[3] is PropertySetterAccess<string>).IsTrue();
		await That(((MethodInvocation<int>)ordered[0]).Parameter1).IsEqualTo(1);
		await That(((MethodInvocation<int>)ordered[2]).Parameter1).IsEqualTo(2);
		await That(((PropertySetterAccess<string>)ordered[1]).Value).IsEqualTo("x");
		await That(((PropertySetterAccess<string>)ordered[3]).Value).IsEqualTo("y");
	}

	[Fact]
	public async Task GetOrCreateFallbackBuffer_ConcurrentRace_RecordsAllInteractions()
	{
		const int threads = 8;
		const int callsPerThread = 25;
		IMockInteractions sut = new FastMockInteractions(0);

		using ManualResetEventSlim start = new(false);
		Task[] tasks = new Task[threads];
		for (int t = 0; t < threads; t++)
		{
			int threadId = t;
#pragma warning disable xUnit1051 // intentionally not using a cancellation token here; the threads run a short, deterministic loop
			tasks[t] = Task.Run(() =>
#pragma warning restore xUnit1051
			{
				start.Wait();
				for (int i = 0; i < callsPerThread; i++)
				{
					sut.RegisterInteraction(new MethodInvocation($"t{threadId}-{i}"));
				}
			});
		}

		start.Set();
		await Task.WhenAll(tasks);

		await That(sut.Count).IsEqualTo(threads * callsPerThread);
		await That(sut.ToList()).HasCount(threads * callsPerThread);
	}

	[Fact]
	public async Task GetUnverifiedInteractions_AcrossBuffersWithInterleavedAppends_ReturnsInSequenceOrder()
	{
		// Pins the `unverified.Count > 1` boundary that gates the defensive Sort by Seq inside
		// GetUnverifiedInteractions. With multiple buffers contributing in non-Seq order (each
		// buffer is iterated in install order, but appends are interleaved across the global
		// sequence), the Sort step is what restores chronological order. Mutations that skip the
		// Sort (`< 1`, `<= 1`, `!(>1)`) leave the snapshot grouped by buffer, not by Seq.
		FastMockInteractions sut = new(2);
		FastMethod1Buffer<int> bufA = sut.InstallMethod<int>(0);
		FastMethod1Buffer<int> bufB = sut.InstallMethod<int>(1);

		bufA.Append("A", 0);
		bufB.Append("B", 1);
		bufA.Append("A", 2);
		bufB.Append("B", 3);

		IInteraction[] unverified = sut.GetUnverifiedInteractions().ToArray();

		await That(unverified).HasCount(4);
		await That(((MethodInvocation<int>)unverified[0]).Parameter1).IsEqualTo(0);
		await That(((MethodInvocation<int>)unverified[1]).Parameter1).IsEqualTo(1);
		await That(((MethodInvocation<int>)unverified[2]).Parameter1).IsEqualTo(2);
		await That(((MethodInvocation<int>)unverified[3]).Parameter1).IsEqualTo(3);
	}

	[Fact]
	public async Task GetUnverifiedInteractions_AcrossMultipleBuffers_ShouldUnionFastAndSlowVerifications()
	{
		FastMockInteractions sut = new(2);
		FastMethod1Buffer<int> methodBuffer = sut.InstallMethod<int>(0);
		FastPropertyGetterBuffer getterBuffer = sut.InstallPropertyGetter(1);

		methodBuffer.Append("M", 7);
		getterBuffer.Append("Prop");

		_ = methodBuffer.ConsumeMatching((IParameterMatch<int>)It.Is(7));
		_ = getterBuffer.ConsumeMatching();

		await That(sut.GetUnverifiedInteractions()).IsEmpty();
	}

	[Fact]
	public async Task GetUnverifiedInteractions_AfterMatcherLessConsumeMatching_ShouldDropMarkedSlots()
	{
		FastMockInteractions sut = new(1);
		FastMethod0Buffer methodBuffer = sut.InstallMethod(0);
		methodBuffer.Append("first");
		methodBuffer.Append("second");

		_ = methodBuffer.ConsumeMatching();

		await That(sut.GetUnverifiedInteractions()).IsEmpty();

		methodBuffer.Append("third");

		IReadOnlyCollection<IInteraction> unverified = sut.GetUnverifiedInteractions();
		await That(unverified).HasCount(1);
		await That(((MethodInvocation)unverified.Single()).Name).IsEqualTo("third");
	}

	[Fact]
	public async Task GetUnverifiedInteractions_AfterTypedConsumeMatching_ShouldDropOnlyMatchedSlots()
	{
		FastMockInteractions sut = new(1);
		FastMethod1Buffer<int> methodBuffer = sut.InstallMethod<int>(0);
		methodBuffer.Append("Dispense", 1);
		methodBuffer.Append("Dispense", 2);

		_ = methodBuffer.ConsumeMatching((IParameterMatch<int>)It.Is(1));

		IReadOnlyCollection<IInteraction> unverified = sut.GetUnverifiedInteractions();
		await That(unverified).HasCount(1);
		await That(((MethodInvocation<int>)unverified.Single()).Parameter1).IsEqualTo(2);

		_ = methodBuffer.ConsumeMatching((IParameterMatch<int>)It.Is(2));

		await That(sut.GetUnverifiedInteractions()).IsEmpty();
	}

	[Fact]
	public async Task GetUnverifiedInteractions_FastPathPlusSlowPath_ShouldFilterByBoth()
	{
		FastMockInteractions sut = new(1);
		FastMethod1Buffer<int> methodBuffer = sut.InstallMethod<int>(0);
		methodBuffer.Append("M", 1);
		methodBuffer.Append("M", 2);
		methodBuffer.Append("M", 3);

		_ = methodBuffer.ConsumeMatching((IParameterMatch<int>)It.Is(1));

		List<IInteraction> all = [..sut,];
		((IMockInteractions)sut).Verified([all[1],]);

		IReadOnlyCollection<IInteraction> unverified = sut.GetUnverifiedInteractions();
		await That(unverified).HasCount(1);
		await That(((MethodInvocation<int>)unverified.Single()).Parameter1).IsEqualTo(3);
	}

	[Fact]
	public async Task GetUnverifiedInteractions_ShouldRespectVerifiedSet()
	{
		FastMockInteractions sut = new(1);
		FastMethod0Buffer methodBuffer = sut.InstallMethod(0);
		methodBuffer.Append("first");
		methodBuffer.Append("second");

		List<IInteraction> all = [..sut,];
		((IMockInteractions)sut).Verified([all[0],]);

		IReadOnlyCollection<IInteraction> unverified = sut.GetUnverifiedInteractions();

		await That(unverified).HasCount(1);
		await That(unverified.Contains(all[0])).IsFalse();
		await That(unverified.Contains(all[1])).IsTrue();
	}

	[Fact]
	public async Task GetUnverifiedInteractions_WhenNothingVerified_ShouldReturnEverything()
	{
		FastMockInteractions sut = new(1);
		FastMethod0Buffer methodBuffer = sut.InstallMethod(0);
		methodBuffer.Append("first");

		IReadOnlyCollection<IInteraction> unverified = sut.GetUnverifiedInteractions();

		await That(unverified).HasCount(1);
	}

	[Fact]
	public async Task NextSequence_ShouldReturnZeroBasedMonotonicValues()
	{
		FastMockInteractions sut = new(0);

		long first = sut.NextSequence();
		long second = sut.NextSequence();
		long third = sut.NextSequence();

		await That(first).IsEqualTo(0L);
		await That(second).IsEqualTo(1L);
		await That(third).IsEqualTo(2L);
	}

	[Fact]
	public async Task RegisterInteraction_FallbackPath_GetUnverifiedInteractions_ShouldRespectVerifiedSet()
	{
		IMockInteractions sut = new FastMockInteractions(0);
		MethodInvocation first = new("first");
		MethodInvocation second = new("second");
		sut.RegisterInteraction(first);
		sut.RegisterInteraction(second);

		sut.Verified([first,]);

		IReadOnlyCollection<IInteraction> unverified = sut.GetUnverifiedInteractions();

		await That(unverified).HasCount(1);
		await That(unverified.Contains(first)).IsFalse();
		await That(unverified.Contains(second)).IsTrue();
	}

	[Fact]
	public async Task RegisterInteraction_FallbackPath_OnClearing_ShouldFireWhenClearIsInvoked()
	{
		int invocations = 0;
		IMockInteractions sut = new FastMockInteractions(0);
		sut.RegisterInteraction(new MethodInvocation("foo"));
		sut.OnClearing += Handler;

		sut.Clear();

		sut.OnClearing -= Handler;

		await That(invocations).IsEqualTo(1);
		await That(sut.Count).IsEqualTo(0);

		void Handler(object? sender, EventArgs e)
		{
			invocations++;
		}
	}

	[Fact]
	public async Task RegisterInteraction_FallbackPath_ShouldEnumerateMultipleInRegistrationOrder()
	{
		IMockInteractions sut = new FastMockInteractions(0);
		MethodInvocation first = new("first");
		MethodInvocation second = new("second");

		sut.RegisterInteraction(first);
		sut.RegisterInteraction(second);

		List<IInteraction> enumerated = [.. sut,];

		await That(sut.Count).IsEqualTo(2);
		await That(enumerated).HasCount(2);
		await That(enumerated[0]).IsSameAs(first);
		await That(enumerated[1]).IsSameAs(second);
	}

	[Fact]
	public async Task RegisterInteraction_FallbackPath_ShouldFireInteractionAddedPerCall()
	{
		int invocations = 0;
		IMockInteractions sut = new FastMockInteractions(0);
		sut.InteractionAdded += Handler;

		sut.RegisterInteraction(new MethodInvocation("foo"));
		sut.RegisterInteraction(new MethodInvocation("bar"));

		sut.InteractionAdded -= Handler;

		await That(invocations).IsEqualTo(2);

		void Handler(object? sender, EventArgs e)
		{
			invocations++;
		}
	}

	[Fact]
	public async Task RegisterInteraction_FallbackPath_ShouldRecordAndEnumerate()
	{
		IMockInteractions sut = new FastMockInteractions(0);
		MethodInvocation interaction = new("foo");

		MethodInvocation registered = sut.RegisterInteraction(interaction);

		await That(registered).IsSameAs(interaction);
		await That(sut.Count).IsEqualTo(1);
		await That(sut.ToList()[0]).IsSameAs(interaction);
	}

	[Fact]
	public async Task RegisterInteraction_WhenSkipInteractionRecording_ShouldNotRecord()
	{
		IMockInteractions sut = new FastMockInteractions(0, true);

		sut.RegisterInteraction(new MethodInvocation("foo"));

		await That(sut.Count).IsEqualTo(0);
	}

	[Fact]
	public async Task SkipInteractionRecording_ShouldReflectConstructionValue()
	{
		FastMockInteractions skipping = new(0, true);
		FastMockInteractions recording = new(0);

		await That(skipping.SkipInteractionRecording).IsTrue();
		await That(recording.SkipInteractionRecording).IsFalse();
	}

	[Fact]
	public async Task Verified_BeforeAnyAppend_OnSecondCallAddsToExistingSet()
	{
		FastMockInteractions sut = new(1);
		FastMethod0Buffer buffer = sut.InstallMethod(0);

		((IMockInteractions)sut).Verified([]);

		buffer.Append("Foo");
		MethodInvocation appended = (MethodInvocation)sut.Single();
		((IMockInteractions)sut).Verified([appended,]);

		await That(sut.GetUnverifiedInteractions()).IsEmpty();
	}

	[Fact]
	public async Task Verified_OnSecondCallWithDifferentItems_PreservesPreviouslyVerifiedItems()
	{
		// Pins the `_verified ??= []` lazy-init in Verified(). With the assignment turned into a
		// plain `_verified = []`, the second call would reset the verified set, so the first
		// call's entry would re-surface as "unverified".
		FastMockInteractions sut = new(1);
		FastMethod0Buffer buffer = sut.InstallMethod(0);
		buffer.Append("first");
		buffer.Append("second");
		List<IInteraction> all = [..sut,];

		((IMockInteractions)sut).Verified([all[0],]);
		((IMockInteractions)sut).Verified([all[1],]);

		await That(sut.GetUnverifiedInteractions()).IsEmpty();
	}

	[Fact]
	public async Task Verified_WithMultipleInteractions_AddsAllToInternalSet()
	{
		FastMockInteractions sut = new(1);
		FastMethod0Buffer buffer = sut.InstallMethod(0);
		buffer.Append("first");
		buffer.Append("second");

		List<IInteraction> all = [..sut,];
		((IMockInteractions)sut).Verified(all);

		await That(sut.GetUnverifiedInteractions()).IsEmpty();
	}

	public sealed class MethodBufferTests
	{
		[Fact]
		public async Task Method0_BoxesAsMethodInvocation()
		{
			FastMockInteractions store = new(1);
			FastMethod0Buffer buffer = store.InstallMethod(0);

			buffer.Append("Foo");

			MethodInvocation boxed = (MethodInvocation)store.Single();
			await That(boxed.Name).IsEqualTo("Foo");
		}

		[Fact]
		public async Task Method1_BoxesAsMethodInvocationT1()
		{
			FastMockInteractions store = new(1);
			FastMethod1Buffer<int> buffer = store.InstallMethod<int>(0);

			buffer.Append("Foo", 42);

			MethodInvocation<int> boxed = (MethodInvocation<int>)store.Single();
			await That(boxed.Name).IsEqualTo("Foo");
			await That(boxed.Parameter1).IsEqualTo(42);
		}

		[Fact]
		public async Task Method2_BoxesAsMethodInvocationT1T2()
		{
			FastMockInteractions store = new(1);
			FastMethod2Buffer<int, string> buffer = store.InstallMethod<int, string>(0);

			buffer.Append("Foo", 42, "bar");

			MethodInvocation<int, string> boxed = (MethodInvocation<int, string>)store.Single();
			await That(boxed.Parameter1).IsEqualTo(42);
			await That(boxed.Parameter2).IsEqualTo("bar");
		}

		[Fact]
		public async Task Method3_BoxesAsMethodInvocationT1T2T3()
		{
			FastMockInteractions store = new(1);
			FastMethod3Buffer<int, string, bool> buffer = store.InstallMethod<int, string, bool>(0);

			buffer.Append("Foo", 1, "two", true);

			MethodInvocation<int, string, bool> boxed = (MethodInvocation<int, string, bool>)store.Single();
			await That(boxed.Parameter1).IsEqualTo(1);
			await That(boxed.Parameter2).IsEqualTo("two");
			await That(boxed.Parameter3).IsTrue();
		}

		[Fact]
		public async Task Method4_BoxesAsMethodInvocationT1T2T3T4()
		{
			FastMockInteractions store = new(1);
			FastMethod4Buffer<int, string, bool, double> buffer =
				store.InstallMethod<int, string, bool, double>(0);

			buffer.Append("Foo", 1, "two", true, 3.14);

			MethodInvocation<int, string, bool, double> boxed =
				(MethodInvocation<int, string, bool, double>)store.Single();
			await That(boxed.Parameter4).IsEqualTo(3.14);
		}

		[Fact]
		public async Task ResizesAndPreservesOrder()
		{
			FastMockInteractions store = new(1);
			FastMethod1Buffer<int> buffer = store.InstallMethod<int>(0);

			for (int i = 0; i < 100; i++)
			{
				buffer.Append("Foo", i);
			}

			await That(buffer.Count).IsEqualTo(100);
			List<IInteraction> ordered = [..store,];
			for (int i = 0; i < 100; i++)
			{
				await That(((MethodInvocation<int>)ordered[i]).Parameter1).IsEqualTo(i);
			}
		}
	}

	public sealed class PropertyBufferTests
	{
		[Fact]
		public async Task Getter_BoxesAsPropertyGetterAccess()
		{
			FastMockInteractions store = new(1);
			FastPropertyGetterBuffer buffer = store.InstallPropertyGetter(0);

			buffer.Append("Foo");

			PropertyGetterAccess boxed = (PropertyGetterAccess)store.Single();
			await That(boxed.Name).IsEqualTo("Foo");
		}

		[Fact]
		public async Task Setter_BoxesAsPropertySetterAccessT()
		{
			FastMockInteractions store = new(1);
			FastPropertySetterBuffer<string> buffer = store.InstallPropertySetter<string>(0);

			buffer.Append("Foo", "value");

			PropertySetterAccess<string> boxed = (PropertySetterAccess<string>)store.Single();
			await That(boxed.Name).IsEqualTo("Foo");
			await That(boxed.Value).IsEqualTo("value");
		}
	}

	public sealed class IndexerBufferTests
	{
		[Fact]
		public async Task Getter1_BoxesAsIndexerGetterAccess()
		{
			FastMockInteractions store = new(1);
			FastIndexerGetterBuffer<string> buffer = store.InstallIndexerGetter<string>(0);

			buffer.Append("k");

			IndexerGetterAccess<string> boxed = (IndexerGetterAccess<string>)store.Single();
			await That(boxed.Parameter1).IsEqualTo("k");
		}

		[Fact]
		public async Task Getter2_BoxesAsIndexerGetterAccess()
		{
			FastMockInteractions store = new(1);
			FastIndexerGetterBuffer<string, int> buffer = store.InstallIndexerGetter<string, int>(0);

			buffer.Append("k", 1);

			IndexerGetterAccess<string, int> boxed = (IndexerGetterAccess<string, int>)store.Single();
			await That(boxed.Parameter1).IsEqualTo("k");
			await That(boxed.Parameter2).IsEqualTo(1);
		}

		[Fact]
		public async Task Setter1_BoxesAsIndexerSetterAccess()
		{
			FastMockInteractions store = new(1);
			FastIndexerSetterBuffer<string, bool> buffer = store.InstallIndexerSetter<string, bool>(0);

			buffer.Append("k", true);

			IndexerSetterAccess<string, bool> boxed = (IndexerSetterAccess<string, bool>)store.Single();
			await That(boxed.Parameter1).IsEqualTo("k");
			await That(boxed.TypedValue).IsTrue();
		}

		[Fact]
		public async Task Setter2_BoxesAsIndexerSetterAccess()
		{
			FastMockInteractions store = new(1);
			FastIndexerSetterBuffer<string, int, bool> buffer =
				store.InstallIndexerSetter<string, int, bool>(0);

			buffer.Append("k", 7, true);

			IndexerSetterAccess<string, int, bool> boxed = (IndexerSetterAccess<string, int, bool>)store.Single();
			await That(boxed.Parameter2).IsEqualTo(7);
		}
	}

	public sealed class EventBufferTests
	{
		[Fact]
		public async Task Subscribe_BoxesAsEventSubscription()
		{
			FastMockInteractions store = new(1);
			FastEventBuffer buffer = store.InstallEventSubscribe(0);
			MethodInfo method = typeof(EventBufferTests).GetMethod(nameof(Subscribe_BoxesAsEventSubscription))!;

			buffer.Append("E", this, method);

			EventSubscription boxed = (EventSubscription)store.Single();
			await That(boxed.Name).IsEqualTo("E");
			await That(boxed.Target).IsSameAs(this);
			await That(boxed.Method).IsSameAs(method);
		}

		[Fact]
		public async Task Unsubscribe_BoxesAsEventUnsubscription()
		{
			FastMockInteractions store = new(1);
			FastEventBuffer buffer = store.InstallEventUnsubscribe(0);
			MethodInfo method = typeof(EventBufferTests).GetMethod(nameof(Unsubscribe_BoxesAsEventUnsubscription))!;

			buffer.Append("E", null, method);

			EventUnsubscription boxed = (EventUnsubscription)store.Single();
			await That(boxed.Name).IsEqualTo("E");
		}
	}

	public sealed class ConcurrencyTests
	{
		[Fact]
		public async Task ConcurrentAppendsAcrossMembers_ShouldPreserveTotalCount()
		{
			const int threads = 8;
			const int callsPerThread = 200;
			FastMockInteractions store = new(threads);
			FastMethod1Buffer<int>[] buffers = new FastMethod1Buffer<int>[threads];
			for (int t = 0; t < threads; t++)
			{
				buffers[t] = store.InstallMethod<int>(t);
			}

			using ManualResetEventSlim start = new(false);
			Task[] tasks = new Task[threads];
			for (int t = 0; t < threads; t++)
			{
				int memberIndex = t;
#pragma warning disable xUnit1051 // intentionally not using a cancellation token here; the threads run a short, deterministic loop
				tasks[t] = Task.Run(() =>
#pragma warning restore xUnit1051
				{
					start.Wait();
					for (int i = 0; i < callsPerThread; i++)
					{
						buffers[memberIndex].Append("M", i);
					}
				});
			}

			start.Set();
			await Task.WhenAll(tasks);

			await That(store.Count).IsEqualTo(threads * callsPerThread);
			int sum = 0;
			foreach (FastMethod1Buffer<int> buffer in buffers)
			{
				sum += buffer.Count;
			}

			await That(sum).IsEqualTo(threads * callsPerThread);
		}
	}
}
