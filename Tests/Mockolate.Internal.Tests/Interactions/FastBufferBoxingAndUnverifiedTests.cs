using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Internal.Tests.Interactions;

public class FastBufferBoxingAndUnverifiedTests
{
	[Fact]
	public async Task FastEventBuffer_AppendBoxed_KindMatters()
	{
		FastMockInteractions subscribeStore = new(1);
		FastMockInteractions unsubscribeStore = new(1);
		FastEventBuffer subscribe = subscribeStore.InstallEventSubscribe(0);
		FastEventBuffer unsubscribe = unsubscribeStore.InstallEventUnsubscribe(0);

		subscribe.Append("E", this, SampleMethod);
		unsubscribe.Append("E", this, SampleMethod);

		await That(subscribeStore.Single()).IsExactly<EventSubscription>();
		await That(unsubscribeStore.Single()).IsExactly<EventUnsubscription>();
	}

	[Fact]
	public async Task FastEventBuffer_AppendBoxedUnverified_ShouldSkipMatchedSlots()
	{
		FastMockInteractions store = new(1);
		FastEventBuffer buffer = store.InstallEventSubscribe(0);
		buffer.Append("E", this, SampleMethod);
		buffer.Append("E", this, SampleMethod);
		buffer.Append("E", this, SampleMethod);

		_ = buffer.ConsumeMatching();

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).IsEmpty();

		buffer.Append("E", this, SampleMethod);
		dest.Clear();
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(1);
		await That(dest[0].Interaction).IsExactly<EventSubscription>();
	}

	[Fact]
	public async Task FastEventBuffer_Subscribe_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		// Pins the `r.Boxed ??= new EventSubscription(...)` cache in AppendBoxed. With the
		// `??=` mutated to `=`, every AppendBoxed call would allocate a fresh record.
		FastMockInteractions store = new(1);
		FastEventBuffer buffer = store.InstallEventSubscribe(0);
		buffer.Append("E", this, SampleMethod);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastEventBuffer_Subscribe_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		// Pins the `r.Boxed ??= new EventSubscription(...)` cache in AppendBoxedUnverified.
		FastMockInteractions store = new(1);
		FastEventBuffer buffer = store.InstallEventSubscribe(0);
		buffer.Append("E", this, SampleMethod);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastEventBuffer_SubscribeAppend_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastEventBuffer buffer = store.InstallEventSubscribe(0);
			return () => buffer.Append("E", this, SampleMethod);
		});

	[Fact]
	public async Task FastEventBuffer_Unsubscribe_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastEventBuffer buffer = store.InstallEventUnsubscribe(0);
		buffer.Append("E", this, SampleMethod);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastEventBuffer_Unsubscribe_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastEventBuffer buffer = store.InstallEventUnsubscribe(0);
		buffer.Append("E", this, SampleMethod);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastEventBuffer_Unsubscribe_AppendBoxedUnverified_ShouldBoxAsEventUnsubscription()
	{
		// Pins the `_kind == FastEventBufferKind.Subscribe ? new EventSubscription(...) :
		// new EventUnsubscription(...)` ternary in AppendBoxedUnverified. With the conditional
		// flipped to `(true ? Subscription : Unsubscription)`, an unsubscribe buffer would
		// surface its records as EventSubscription.
		FastMockInteractions store = new(1);
		FastEventBuffer buffer = store.InstallEventUnsubscribe(0);
		buffer.Append("E", this, SampleMethod);

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(1);
		await That(dest[0].Interaction).IsExactly<EventUnsubscription>();
	}

	[Fact]
	public async Task FastEventBuffer_UnsubscribeAppend_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastEventBuffer buffer = store.InstallEventUnsubscribe(0);
			return () => buffer.Append("E", this, SampleMethod);
		});

	[Fact]
	public async Task FastIndexerGetterBuffer1_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastIndexerGetterBuffer<int> buffer = store.InstallIndexerGetter<int>(0);
			return () => buffer.Append(1);
		});

	[Fact]
	public async Task FastIndexerGetterBuffer1_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int> buffer = store.InstallIndexerGetter<int>(0);

		buffer.Append(7);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerGetterBuffer1_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int> buffer = store.InstallIndexerGetter<int>(0);

		buffer.Append(7);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerGetterBuffer1_AppendWithAccess_ShouldPublishAndRaiseInteractionAdded()
		=> await VerifyAppendWithAccessPublishesAndRaises(store =>
		{
			FastIndexerGetterBuffer<int> buffer = store.InstallIndexerGetter<int>(0);
			IndexerGetterAccess<int> access = new(7);
			return (() => buffer.Append(access), () => buffer.Count);
		});

	[Fact]
	public async Task FastIndexerGetterBuffer2_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastIndexerGetterBuffer<int, string> buffer = store.InstallIndexerGetter<int, string>(0);
			return () => buffer.Append(1, "a");
		});

	[Fact]
	public async Task FastIndexerGetterBuffer2_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string> buffer = store.InstallIndexerGetter<int, string>(0);

		buffer.Append(7, "k");

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerGetterBuffer2_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string> buffer = store.InstallIndexerGetter<int, string>(0);

		buffer.Append(7, "k");

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerGetterBuffer2_AppendWithAccess_ShouldPublishAndRaiseInteractionAdded()
		=> await VerifyAppendWithAccessPublishesAndRaises(store =>
		{
			FastIndexerGetterBuffer<int, string> buffer = store.InstallIndexerGetter<int, string>(0);
			IndexerGetterAccess<int, string> access = new(7, "k");
			return (() => buffer.Append(access), () => buffer.Count);
		});

	[Fact]
	public async Task FastIndexerGetterBuffer3_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastIndexerGetterBuffer<int, string, bool> buffer =
				store.InstallIndexerGetter<int, string, bool>(0);
			return () => buffer.Append(1, "a", true);
		});

	[Fact]
	public async Task FastIndexerGetterBuffer3_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string, bool> buffer =
			store.InstallIndexerGetter<int, string, bool>(0);

		buffer.Append(7, "k", true);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerGetterBuffer3_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string, bool> buffer =
			store.InstallIndexerGetter<int, string, bool>(0);

		buffer.Append(7, "k", true);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerGetterBuffer3_AppendWithAccess_ShouldPublishAndRaiseInteractionAdded()
		=> await VerifyAppendWithAccessPublishesAndRaises(store =>
		{
			FastIndexerGetterBuffer<int, string, bool> buffer =
				store.InstallIndexerGetter<int, string, bool>(0);
			IndexerGetterAccess<int, string, bool> access = new(7, "k", true);
			return (() => buffer.Append(access), () => buffer.Count);
		});

	[Fact]
	public async Task FastIndexerGetterBuffer4_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastIndexerGetterBuffer<int, string, bool, double> buffer =
				store.InstallIndexerGetter<int, string, bool, double>(0);
			return () => buffer.Append(1, "a", true, 1.0);
		});

	[Fact]
	public async Task FastIndexerGetterBuffer4_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string, bool, double> buffer =
			store.InstallIndexerGetter<int, string, bool, double>(0);

		buffer.Append(7, "k", true, 3.14);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerGetterBuffer4_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string, bool, double> buffer =
			store.InstallIndexerGetter<int, string, bool, double>(0);

		buffer.Append(7, "k", true, 3.14);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerGetterBuffer4_AppendWithAccess_ShouldPublishAndRaiseInteractionAdded()
		=> await VerifyAppendWithAccessPublishesAndRaises(store =>
		{
			FastIndexerGetterBuffer<int, string, bool, double> buffer =
				store.InstallIndexerGetter<int, string, bool, double>(0);
			IndexerGetterAccess<int, string, bool, double> access = new(7, "k", true, 3.14);
			return (() => buffer.Append(access), () => buffer.Count);
		});

	[Fact]
	public async Task FastIndexerSetterBuffer1_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastIndexerSetterBuffer<int, string> buffer = store.InstallIndexerSetter<int, string>(0);
			return () => buffer.Append(1, "a");
		});

	[Fact]
	public async Task FastIndexerSetterBuffer1_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string> buffer = store.InstallIndexerSetter<int, string>(0);

		buffer.Append(7, "k");

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerSetterBuffer1_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string> buffer = store.InstallIndexerSetter<int, string>(0);

		buffer.Append(7, "k");

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerSetterBuffer1_AppendWithAccess_ShouldPublishAndRaiseInteractionAdded()
		=> await VerifyAppendWithAccessPublishesAndRaises(store =>
		{
			FastIndexerSetterBuffer<int, string> buffer = store.InstallIndexerSetter<int, string>(0);
			IndexerSetterAccess<int, string> access = new(7, "k");
			return (() => buffer.Append(access), () => buffer.Count);
		});

	[Fact]
	public async Task FastIndexerSetterBuffer2_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastIndexerSetterBuffer<int, string, bool> buffer =
				store.InstallIndexerSetter<int, string, bool>(0);
			return () => buffer.Append(1, "a", true);
		});

	[Fact]
	public async Task FastIndexerSetterBuffer2_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool> buffer =
			store.InstallIndexerSetter<int, string, bool>(0);

		buffer.Append(7, "k", true);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerSetterBuffer2_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool> buffer =
			store.InstallIndexerSetter<int, string, bool>(0);

		buffer.Append(7, "k", true);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerSetterBuffer2_AppendWithAccess_ShouldPublishAndRaiseInteractionAdded()
		=> await VerifyAppendWithAccessPublishesAndRaises(store =>
		{
			FastIndexerSetterBuffer<int, string, bool> buffer =
				store.InstallIndexerSetter<int, string, bool>(0);
			IndexerSetterAccess<int, string, bool> access = new(7, "k", true);
			return (() => buffer.Append(access), () => buffer.Count);
		});

	[Fact]
	public async Task FastIndexerSetterBuffer3_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastIndexerSetterBuffer<int, string, bool, double> buffer =
				store.InstallIndexerSetter<int, string, bool, double>(0);
			return () => buffer.Append(1, "a", true, 1.0);
		});

	[Fact]
	public async Task FastIndexerSetterBuffer3_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool, double> buffer =
			store.InstallIndexerSetter<int, string, bool, double>(0);

		buffer.Append(7, "k", true, 3.14);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerSetterBuffer3_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool, double> buffer =
			store.InstallIndexerSetter<int, string, bool, double>(0);

		buffer.Append(7, "k", true, 3.14);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerSetterBuffer3_AppendWithAccess_ShouldPublishAndRaiseInteractionAdded()
		=> await VerifyAppendWithAccessPublishesAndRaises(store =>
		{
			FastIndexerSetterBuffer<int, string, bool, double> buffer =
				store.InstallIndexerSetter<int, string, bool, double>(0);
			IndexerSetterAccess<int, string, bool, double> access = new(7, "k", true, 3.14);
			return (() => buffer.Append(access), () => buffer.Count);
		});

	[Fact]
	public async Task FastIndexerSetterBuffer4_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastIndexerSetterBuffer<int, string, bool, double, char> buffer =
				store.InstallIndexerSetter<int, string, bool, double, char>(0);
			return () => buffer.Append(1, "a", true, 1.0, 'x');
		});

	[Fact]
	public async Task FastIndexerSetterBuffer4_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool, double, char> buffer =
			store.InstallIndexerSetter<int, string, bool, double, char>(0);

		buffer.Append(7, "k", true, 3.14, 'z');

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerSetterBuffer4_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool, double, char> buffer =
			store.InstallIndexerSetter<int, string, bool, double, char>(0);

		buffer.Append(7, "k", true, 3.14, 'z');

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastIndexerSetterBuffer4_AppendWithAccess_ShouldPublishAndRaiseInteractionAdded()
		=> await VerifyAppendWithAccessPublishesAndRaises(store =>
		{
			FastIndexerSetterBuffer<int, string, bool, double, char> buffer =
				store.InstallIndexerSetter<int, string, bool, double, char>(0);
			IndexerSetterAccess<int, string, bool, double, char> access = new(7, "k", true, 3.14, 'z');
			return (() => buffer.Append(access), () => buffer.Count);
		});

	[Fact]
	public async Task FastMethod0Buffer_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		// Pins the `r.Boxed ??= new MethodInvocation(r.Name)` cache in Method0 AppendBoxed.
		FastMockInteractions store = new(1);
		FastMethod0Buffer buffer = store.InstallMethod(0);

		buffer.Append("M");

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastMethod1Buffer_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastMethod1Buffer<int> buffer = store.InstallMethod<int>(0);
			return () => buffer.Append("M", 1);
		});

	[Fact]
	public async Task FastMethod1Buffer_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastMethod1Buffer<int> buffer = store.InstallMethod<int>(0);

		buffer.Append("M", 1);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastMethod2Buffer_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastMethod2Buffer<int, string> buffer = store.InstallMethod<int, string>(0);
			return () => buffer.Append("M", 1, "a");
		});

	[Fact]
	public async Task FastMethod3Buffer_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastMethod3Buffer<int, string, bool> buffer = store.InstallMethod<int, string, bool>(0);
			return () => buffer.Append("M", 1, "a", true);
		});

	[Fact]
	public async Task FastMethod3Buffer_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		// Pins the `r.Boxed ??= new MethodInvocation<T1,T2,T3>(...)` cache in Method3 AppendBoxed.
		FastMockInteractions store = new(1);
		FastMethod3Buffer<int, string, bool> buffer = store.InstallMethod<int, string, bool>(0);

		buffer.Append("M", 1, "a", true);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastMethod3Buffer_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		// Pins the `r.Boxed ??= new MethodInvocation<T1,T2,T3>(...)` cache in Method3 AppendBoxedUnverified.
		FastMockInteractions store = new(1);
		FastMethod3Buffer<int, string, bool> buffer = store.InstallMethod<int, string, bool>(0);

		buffer.Append("M", 1, "a", true);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastMethod3Buffer_AppendBoxedUnverified_ShouldSkipMatchedSlots()
	{
		FastMockInteractions store = new(1);
		FastMethod3Buffer<int, string, bool> buffer = store.InstallMethod<int, string, bool>(0);
		buffer.Append("M", 0, "a", true);
		buffer.Append("M", 1, "b", false);
		buffer.Append("M", 2, "c", true);

		_ = buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("b"),
			(IParameterMatch<bool>)It.Is(false));

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(2);
		await That(((MethodInvocation<int, string, bool>)dest[0].Interaction).Parameter1).IsEqualTo(0);
		await That(((MethodInvocation<int, string, bool>)dest[1].Interaction).Parameter1).IsEqualTo(2);
	}

	[Fact]
	public async Task FastMethod4Buffer_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastMethod4Buffer<int, string, bool, double> buffer =
				store.InstallMethod<int, string, bool, double>(0);
			return () => buffer.Append("M", 1, "a", true, 1.0);
		});

	[Fact]
	public async Task FastMethod4Buffer_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		// Pins the `r.Boxed ??= new MethodInvocation<T1,T2,T3,T4>(...)` cache in Method4 AppendBoxed.
		FastMockInteractions store = new(1);
		FastMethod4Buffer<int, string, bool, double> buffer =
			store.InstallMethod<int, string, bool, double>(0);

		buffer.Append("M", 1, "a", true, 1.0);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastMethod4Buffer_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		// Pins the `r.Boxed ??= new MethodInvocation<T1,T2,T3,T4>(...)` cache in Method4 AppendBoxedUnverified.
		FastMockInteractions store = new(1);
		FastMethod4Buffer<int, string, bool, double> buffer =
			store.InstallMethod<int, string, bool, double>(0);

		buffer.Append("M", 1, "a", true, 1.0);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastMethod4Buffer_AppendBoxedUnverified_ShouldSkipMatchedSlots()
	{
		FastMockInteractions store = new(1);
		FastMethod4Buffer<int, string, bool, double> buffer =
			store.InstallMethod<int, string, bool, double>(0);
		buffer.Append("M", 0, "a", true, 0.5);
		buffer.Append("M", 1, "b", false, 1.5);
		buffer.Append("M", 2, "c", true, 2.5);

		_ = buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("b"),
			(IParameterMatch<bool>)It.Is(false),
			(IParameterMatch<double>)It.Is(1.5));

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(2);
		await That(((MethodInvocation<int, string, bool, double>)dest[0].Interaction).Parameter1).IsEqualTo(0);
		await That(((MethodInvocation<int, string, bool, double>)dest[1].Interaction).Parameter1).IsEqualTo(2);
	}

	[Fact]
	public async Task FastPropertyGetterBuffer_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastPropertyGetterBuffer buffer = store.InstallPropertyGetter(0);
			return () => buffer.Append("P");
		});

	[Fact]
	public async Task FastPropertyGetterBuffer_Append_WithoutInstalledSingleton_ShouldIncludeFactoryGuidanceInMessage()
	{
		// Kills the `$"..."` -> `$""` string mutation on the InvalidOperationException message.
		// The message points callers at the correct factory overload — without it, the failure
		// is just an empty-string IOE, which is useless guidance.
		FastMockInteractions store = new(1);
		FastPropertyGetterBuffer buffer = store.InstallPropertyGetter(0);

		void Act()
		{
			buffer.Append();
		}

		await That(Act).Throws<InvalidOperationException>()
			.WithMessage("*InstallPropertyGetter*").AsWildcard();
	}

	[Fact]
	public async Task FastPropertyGetterBuffer_Append_WithoutInstalledSingleton_ShouldThrow()
	{
		FastMockInteractions store = new(1);
		FastPropertyGetterBuffer buffer = store.InstallPropertyGetter(0);

		void Act()
		{
			buffer.Append();
		}

		await That(Act).Throws<InvalidOperationException>();
	}

	[Fact]
	public async Task FastPropertyGetterBuffer_AppendBoxed_RepeatedCallsReturnSameSingleton()
	{
		FastMockInteractions store = new(1);
		FastPropertyGetterBuffer buffer = store.InstallPropertyGetter(0);

		buffer.Append("P");

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastPropertyGetterBuffer_AppendBoxed_SharesSingletonAcrossRecords()
	{
		// All recorded getter accesses for the same property surface as one PropertyGetterAccess
		// reference. This is intentional — getters carry no parameters, so every record is
		// semantically identical, and the two reference-keyed verification paths
		// (FastMockInteractions._verified and VerificationResultExtensions.Then) tolerate
		// shared identity (the Then walker is positional, the verified filter is
		// all-or-nothing per matched property).
		FastMockInteractions store = new(1);
		FastPropertyGetterBuffer buffer = store.InstallPropertyGetter(0);

		buffer.Append("P");
		buffer.Append("P");

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxed(dest);

		await That(dest).HasCount(2);
		await That(dest[0].Interaction).IsSameAs(dest[1].Interaction);
	}

	[Fact]
	public async Task FastPropertyGetterBuffer_AppendString_LazyInitsAccessOnce()
	{
		FastMockInteractions store = new(1);
		FastPropertyGetterBuffer buffer = store.InstallPropertyGetter(0);

		buffer.Append("P");
		List<(long Seq, IInteraction Interaction)> firstSnapshot = [];
		((IFastMemberBuffer)buffer).AppendBoxed(firstSnapshot);

		buffer.Append("P");
		List<(long Seq, IInteraction Interaction)> secondSnapshot = [];
		((IFastMemberBuffer)buffer).AppendBoxed(secondSnapshot);

		await That(firstSnapshot).HasCount(1);
		await That(secondSnapshot).HasCount(2);
		await That(secondSnapshot[0].Interaction).IsSameAs(firstSnapshot[0].Interaction);
		await That(secondSnapshot[1].Interaction).IsSameAs(firstSnapshot[0].Interaction);
	}

	[Fact]
	public async Task FastPropertySetterBuffer_Append_ShouldRaiseInteractionAdded()
		=> await VerifyRaisesInteractionAdded(store =>
		{
			FastPropertySetterBuffer<int> buffer = store.InstallPropertySetter<int>(0);
			return () => buffer.Append("P", 1);
		});

	[Fact]
	public async Task FastPropertySetterBuffer_AppendBoxed_CachesAndReusesAlreadyBoxedRecord()
	{
		FastMockInteractions store = new(1);
		FastPropertySetterBuffer<int> buffer = store.InstallPropertySetter<int>(0);

		buffer.Append("P", 5);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxed(first);
		((IFastMemberBuffer)buffer).AppendBoxed(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastPropertySetterBuffer_AppendBoxedUnverified_CachesAndReusesAlreadyBoxedRecord()
	{
		// Mirrors the AppendBoxed caching test for the Unverified path so the `r.Boxed ??= new
		// PropertySetterAccess<T>(...)` mutation is killed there as well.
		FastMockInteractions store = new(1);
		FastPropertySetterBuffer<int> buffer = store.InstallPropertySetter<int>(0);

		buffer.Append("P", 5);

		List<(long Seq, IInteraction Interaction)> first = [];
		List<(long Seq, IInteraction Interaction)> second = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(first);
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(second);

		await That(first).HasCount(1);
		await That(second).HasCount(1);
		await That(second[0].Interaction).IsSameAs(first[0].Interaction);
	}

	[Fact]
	public async Task FastPropertySetterBuffer_AppendBoxedUnverified_ShouldSkipMatchedSlots()
	{
		// Covers the AppendBoxedUnverified branch on the setter buffer (NoCoverage cluster).
		FastMockInteractions store = new(1);
		FastPropertySetterBuffer<int> buffer = store.InstallPropertySetter<int>(0);
		buffer.Append("P", 1);
		buffer.Append("P", 2);
		buffer.Append("P", 3);

		_ = buffer.ConsumeMatching((IParameterMatch<int>)It.Is(2));

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(2);
		await That(((PropertySetterAccess<int>)dest[0].Interaction).Value).IsEqualTo(1);
		await That(((PropertySetterAccess<int>)dest[1].Interaction).Value).IsEqualTo(3);
	}

	[Fact]
	public async Task FastPropertySetterBuffer_ConsumeMatching_ShouldMarkSlotsVerified()
	{
		// Pins the `_storage.VerifiedUnderLock(slot) = true` write. With the mutation flipped
		// to `false`, ConsumeMatching would still report matches but the slots would never be
		// marked verified — so a second ConsumeMatching call would re-count the same records.
		FastMockInteractions store = new(1);
		FastPropertySetterBuffer<int> buffer = store.InstallPropertySetter<int>(0);
		buffer.Append("P", 1);
		buffer.Append("P", 1);

		int firstPass = buffer.ConsumeMatching((IParameterMatch<int>)It.Is(1));
		await That(firstPass).IsEqualTo(2);

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).IsEmpty();
	}

	[Fact]
	public async Task IndexerGetterBuffer1_AppendBoxedUnverified_ShouldSkipMatchedSlots()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int> buffer = store.InstallIndexerGetter<int>(0);
		buffer.Append(0);
		buffer.Append(1);
		buffer.Append(2);

		_ = buffer.ConsumeMatching((IParameterMatch<int>)It.Is(1));

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(2);
		await That(((IndexerGetterAccess<int>)dest[0].Interaction).Parameter1).IsEqualTo(0);
		await That(((IndexerGetterAccess<int>)dest[1].Interaction).Parameter1).IsEqualTo(2);
	}

	[Fact]
	public async Task IndexerGetterBuffer2_AppendBoxedUnverified_ShouldSkipMatchedSlots()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string> buffer = store.InstallIndexerGetter<int, string>(0);
		buffer.Append(0, "a");
		buffer.Append(1, "b");
		buffer.Append(2, "c");

		_ = buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("b"));

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(2);
		await That(((IndexerGetterAccess<int, string>)dest[0].Interaction).Parameter1).IsEqualTo(0);
		await That(((IndexerGetterAccess<int, string>)dest[1].Interaction).Parameter1).IsEqualTo(2);
	}

	[Fact]
	public async Task IndexerGetterBuffer3_AppendBoxed_BoxesAsIndexerGetterAccess()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string, bool> buffer =
			store.InstallIndexerGetter<int, string, bool>(0);

		buffer.Append(7, "k", true);

		IndexerGetterAccess<int, string, bool> boxed =
			(IndexerGetterAccess<int, string, bool>)store.Single();
		await That(boxed.Parameter1).IsEqualTo(7);
		await That(boxed.Parameter2).IsEqualTo("k");
		await That(boxed.Parameter3).IsTrue();
	}

	[Fact]
	public async Task IndexerGetterBuffer3_AppendBoxedUnverified_ShouldSkipMatchedSlots()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string, bool> buffer =
			store.InstallIndexerGetter<int, string, bool>(0);
		buffer.Append(0, "a", true);
		buffer.Append(1, "b", false);
		buffer.Append(2, "c", true);

		_ = buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("b"),
			(IParameterMatch<bool>)It.Is(false));

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(2);
		await That(((IndexerGetterAccess<int, string, bool>)dest[0].Interaction).Parameter1).IsEqualTo(0);
		await That(((IndexerGetterAccess<int, string, bool>)dest[1].Interaction).Parameter1).IsEqualTo(2);
	}

	[Fact]
	public async Task IndexerGetterBuffer4_AppendBoxed_BoxesAsIndexerGetterAccess()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string, bool, double> buffer =
			store.InstallIndexerGetter<int, string, bool, double>(0);

		buffer.Append(7, "k", true, 3.14);

		IndexerGetterAccess<int, string, bool, double> boxed =
			(IndexerGetterAccess<int, string, bool, double>)store.Single();
		await That(boxed.Parameter1).IsEqualTo(7);
		await That(boxed.Parameter2).IsEqualTo("k");
		await That(boxed.Parameter3).IsTrue();
		await That(boxed.Parameter4).IsEqualTo(3.14);
	}

	[Fact]
	public async Task IndexerGetterBuffer4_AppendBoxedUnverified_ShouldSkipMatchedSlots()
	{
		FastMockInteractions store = new(1);
		FastIndexerGetterBuffer<int, string, bool, double> buffer =
			store.InstallIndexerGetter<int, string, bool, double>(0);
		buffer.Append(0, "a", true, 0.5);
		buffer.Append(1, "b", false, 1.5);
		buffer.Append(2, "c", true, 2.5);

		_ = buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("b"),
			(IParameterMatch<bool>)It.Is(false),
			(IParameterMatch<double>)It.Is(1.5));

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(2);
		await That(((IndexerGetterAccess<int, string, bool, double>)dest[0].Interaction).Parameter1).IsEqualTo(0);
		await That(((IndexerGetterAccess<int, string, bool, double>)dest[1].Interaction).Parameter1).IsEqualTo(2);
	}

	[Fact]
	public async Task IndexerSetterBuffer1_AppendBoxedUnverified_ShouldSkipMatchedSlots()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string> buffer = store.InstallIndexerSetter<int, string>(0);
		buffer.Append(0, "x");
		buffer.Append(1, "y");
		buffer.Append(2, "z");

		_ = buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("y"));

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(2);
		await That(((IndexerSetterAccess<int, string>)dest[0].Interaction).Parameter1).IsEqualTo(0);
		await That(((IndexerSetterAccess<int, string>)dest[1].Interaction).Parameter1).IsEqualTo(2);
	}

	[Fact]
	public async Task IndexerSetterBuffer2_AppendBoxedUnverified_ShouldSkipMatchedSlots()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool> buffer =
			store.InstallIndexerSetter<int, string, bool>(0);
		buffer.Append(0, "x", true);
		buffer.Append(1, "y", false);
		buffer.Append(2, "z", true);

		_ = buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("y"),
			(IParameterMatch<bool>)It.Is(false));

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(2);
		await That(((IndexerSetterAccess<int, string, bool>)dest[0].Interaction).Parameter1).IsEqualTo(0);
		await That(((IndexerSetterAccess<int, string, bool>)dest[1].Interaction).Parameter1).IsEqualTo(2);
	}

	[Fact]
	public async Task IndexerSetterBuffer3_AppendBoxed_BoxesAsIndexerSetterAccess()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool, double> buffer =
			store.InstallIndexerSetter<int, string, bool, double>(0);

		buffer.Append(7, "k", true, 3.14);

		IndexerSetterAccess<int, string, bool, double> boxed =
			(IndexerSetterAccess<int, string, bool, double>)store.Single();
		await That(boxed.Parameter1).IsEqualTo(7);
		await That(boxed.Parameter2).IsEqualTo("k");
		await That(boxed.Parameter3).IsTrue();
		await That(boxed.TypedValue).IsEqualTo(3.14);
	}

	[Fact]
	public async Task IndexerSetterBuffer3_AppendBoxedUnverified_ShouldSkipMatchedSlots()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool, double> buffer =
			store.InstallIndexerSetter<int, string, bool, double>(0);
		buffer.Append(0, "x", true, 0.5);
		buffer.Append(1, "y", false, 1.5);
		buffer.Append(2, "z", true, 2.5);

		_ = buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("y"),
			(IParameterMatch<bool>)It.Is(false),
			(IParameterMatch<double>)It.Is(1.5));

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(2);
		await That(((IndexerSetterAccess<int, string, bool, double>)dest[0].Interaction).Parameter1).IsEqualTo(0);
		await That(((IndexerSetterAccess<int, string, bool, double>)dest[1].Interaction).Parameter1).IsEqualTo(2);
	}

	[Fact]
	public async Task IndexerSetterBuffer4_AppendBoxed_BoxesAsIndexerSetterAccess()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool, double, char> buffer =
			store.InstallIndexerSetter<int, string, bool, double, char>(0);

		buffer.Append(7, "k", true, 3.14, 'z');

		IndexerSetterAccess<int, string, bool, double, char> boxed =
			(IndexerSetterAccess<int, string, bool, double, char>)store.Single();
		await That(boxed.Parameter1).IsEqualTo(7);
		await That(boxed.Parameter2).IsEqualTo("k");
		await That(boxed.Parameter3).IsTrue();
		await That(boxed.Parameter4).IsEqualTo(3.14);
		await That(boxed.TypedValue).IsEqualTo('z');
	}

	[Fact]
	public async Task IndexerSetterBuffer4_AppendBoxedUnverified_ShouldSkipMatchedSlots()
	{
		FastMockInteractions store = new(1);
		FastIndexerSetterBuffer<int, string, bool, double, char> buffer =
			store.InstallIndexerSetter<int, string, bool, double, char>(0);
		buffer.Append(0, "x", true, 0.5, 'a');
		buffer.Append(1, "y", false, 1.5, 'b');
		buffer.Append(2, "z", true, 2.5, 'c');

		_ = buffer.ConsumeMatching(
			(IParameterMatch<int>)It.Is(1),
			(IParameterMatch<string>)It.Is<string>("y"),
			(IParameterMatch<bool>)It.Is(false),
			(IParameterMatch<double>)It.Is(1.5),
			(IParameterMatch<char>)It.Is('b'));

		List<(long Seq, IInteraction Interaction)> dest = [];
		((IFastMemberBuffer)buffer).AppendBoxedUnverified(dest);

		await That(dest).HasCount(2);
		await That(((IndexerSetterAccess<int, string, bool, double, char>)dest[0].Interaction).Parameter1).IsEqualTo(0);
		await That(((IndexerSetterAccess<int, string, bool, double, char>)dest[1].Interaction).Parameter1).IsEqualTo(2);
	}

	private static async Task VerifyAppendWithAccessPublishesAndRaises(
		Func<FastMockInteractions, (Action Append, Func<int> CountReader)> setupFactory)
	{
		// The singleton-aware Append(access) overloads each end with three statements that the
		// existing tests never exercise: `_storage.Publish();`, `if (_owner.HasInteractionAddedSubscribers)`,
		// and `_owner.RaiseAdded();`. Asserting both buffer.Count and the InteractionAdded handler
		// runs pins the publish, the guard, and the raise call together — covers the L61/L63/L65
		// mutant cluster across all 8 indexer arity overloads.
		FastMockInteractions store = new(1);
		(Action append, Func<int> countReader) = setupFactory(store);
		int invocations = 0;
		EventHandler handler = (_, _) => invocations++;
		store.InteractionAdded += handler;

		append();
		append();

		store.InteractionAdded -= handler;

		await That(countReader()).IsEqualTo(2);
		await That(invocations).IsEqualTo(2);
	}

	private static readonly MethodInfo SampleMethod =
		typeof(FastBufferBoxingAndUnverifiedTests).GetMethod(
			nameof(SampleMethodImpl),
			BindingFlags.Static | BindingFlags.NonPublic)!;

	private static void SampleMethodImpl()
	{
	}

	private static async Task VerifyRaisesInteractionAdded(Func<FastMockInteractions, Action> appendFactory)
	{
		FastMockInteractions store = new(1);
		Action append = appendFactory(store);
		int invocations = 0;
		EventHandler handler = (_, _) => invocations++;
		store.InteractionAdded += handler;

		append();
		append();

		store.InteractionAdded -= handler;

		await That(invocations).IsEqualTo(2);
	}
}
