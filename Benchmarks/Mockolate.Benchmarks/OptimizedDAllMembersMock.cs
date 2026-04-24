// PREVIEW — D-refactor reference harness (Phase 0). Hand-written model of the per-member-buffer
// recording pattern the generator will emit across every member kind (method, property, indexer,
// event) after Phases 4–6. Will be deleted in Step 7.3 once the real implementation lands.
// Do not depend on any type defined here from outside the benchmark project.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Benchmarks.Optimized;

/// <summary>
///     Demonstrates that the D pattern is uniform across every member kind Mockolate supports:
///     methods (void + return), property getters, property setters, indexer getters, indexer setters,
///     and event subscribe/unsubscribe. Each member gets:
///     <list type="bullet">
///       <item><description>a dense compile-time <c>int</c> member id (the generator assigns one per member);</description></item>
///       <item><description>a typed per-member buffer (struct records, lock-only-on-resize, amortized zero heap alloc per call);</description></item>
///       <item><description>a Verify assertion that walks only that one buffer;</description></item>
///       <item><description>a Monitor-compatible ordered enumeration path via <see cref="FastMockInteractions" />' merged seq stream.</description></item>
///     </list>
///     Getters and setters are distinct member ids so filtered Verify (<c>Verify.MyProp.Got()</c> vs
///     <c>Verify.MyProp.Set(v)</c>) hits a tight buffer. The indexer follows the same rule — one id
///     per (signature × access kind). Event subscribe/unsubscribe also get two ids. This mirrors the
///     structure the generator will emit.
/// </summary>
public interface IAllMembersInterface
{
	bool MyFunc(int value, string name);
	int Counter { get; set; }
	bool this[string key] { get; set; }
	event EventHandler SomeEvent;
}

public sealed class OptimizedDAllMembersMock : IAllMembersInterface
{
	// ---- Generator-emitted member ids (one per mockable "access") ----
	internal const int Id_MyFunc = 0;
	internal const int Id_Counter_Get = 1;
	internal const int Id_Counter_Set = 2;
	internal const int Id_Indexer_Get = 3;
	internal const int Id_Indexer_Set = 4;
	internal const int Id_SomeEvent_Subscribe = 5;
	internal const int Id_SomeEvent_Unsubscribe = 6;
	internal const int MemberCount = 7;

	// ---- Member names — compile-time constants, referenced only by the Monitor boxing path ----
	internal const string Name_MyFunc = "IAllMembersInterface.MyFunc";
	internal const string Name_Counter = "IAllMembersInterface.Counter";
	internal const string Name_Indexer = "IAllMembersInterface.this[]";
	internal const string Name_SomeEvent = "IAllMembersInterface.SomeEvent";

	// Backing state for the non-method members — deliberately minimal. Real setups and
	// the existing PropertySetup/IndexerSetup pipelines would layer over this; the point of
	// this file is to prove the per-member-buffer recording and Verify pattern, not
	// rebuild those pipelines.
	private int _counter;
	private readonly Dictionary<string, bool> _indexerStore = new();
	private EventHandler? _someEvent;

	public OptimizedDAllMembersMock()
	{
		Mock = new OptimizedDAllMembersFacade(MemberCount);
	}

	public OptimizedDAllMembersFacade Mock { get; }

	// ---- HOT PATHS (all seven members emit the same shape) ----

	public bool MyFunc(int value, string name)
	{
		if (!Mock.Behavior.SkipInteractionRecording)
		{
			Mock.Interactions.RecordMethod2<int, string>(Id_MyFunc, Name_MyFunc, value, name);
		}
		// setups lookup + Return value would live here — omitted for brevity; same as OptimizedDMy2ParamMock.
		return default;
	}

	public int Counter
	{
		get
		{
			if (!Mock.Behavior.SkipInteractionRecording)
			{
				Mock.Interactions.RecordPropertyGet(Id_Counter_Get, Name_Counter);
			}

			return _counter;
		}
		set
		{
			if (!Mock.Behavior.SkipInteractionRecording)
			{
				Mock.Interactions.RecordPropertySet(Id_Counter_Set, Name_Counter, value);
			}

			_counter = value;
		}
	}

	public bool this[string key]
	{
		get
		{
			if (!Mock.Behavior.SkipInteractionRecording)
			{
				Mock.Interactions.RecordIndexerGet(Id_Indexer_Get, Name_Indexer, key);
			}

			return _indexerStore.TryGetValue(key, out bool v) && v;
		}
		set
		{
			if (!Mock.Behavior.SkipInteractionRecording)
			{
				Mock.Interactions.RecordIndexerSet(Id_Indexer_Set, Name_Indexer, key, value);
			}

			_indexerStore[key] = value;
		}
	}

	public event EventHandler SomeEvent
	{
		add
		{
			if (!Mock.Behavior.SkipInteractionRecording && value is not null)
			{
				Mock.Interactions.RecordEvent(Id_SomeEvent_Subscribe, Name_SomeEvent, value.Target, value.Method);
			}

			_someEvent += value;
		}
		remove
		{
			if (!Mock.Behavior.SkipInteractionRecording && value is not null)
			{
				Mock.Interactions.RecordEvent(Id_SomeEvent_Unsubscribe, Name_SomeEvent, value.Target, value.Method);
			}

			_someEvent -= value;
		}
	}

	// Test helper — fire the event as the mocked subject would from inside.
	internal void RaiseSomeEvent(object? sender, EventArgs e) => _someEvent?.Invoke(sender, e);

	// Bench helpers — seed backing state without recording an interaction (mirrors
	// Mockolate's `Setup.Counter.InitializeWith(42)` which also bypasses the recording path).
	public void InitializeCounter(int value) => _counter = value;

	public void InitializeIndexer(string key, bool value) => _indexerStore[key] = value;
}

public sealed class OptimizedDAllMembersFacade
{
	internal OptimizedDAllMembersFacade(int memberCount)
	{
		Interactions = new FastAllMembersInteractions(memberCount);

		// Wire per-member buffers at construction — the generator knows each member's shape.
		Interactions.EnsureMethod2<int, string>(OptimizedDAllMembersMock.Id_MyFunc);
		Interactions.EnsurePropertyGet(OptimizedDAllMembersMock.Id_Counter_Get);
		Interactions.EnsurePropertySet<int>(OptimizedDAllMembersMock.Id_Counter_Set);
		Interactions.EnsureIndexerGet<string>(OptimizedDAllMembersMock.Id_Indexer_Get);
		Interactions.EnsureIndexerSet<string, bool>(OptimizedDAllMembersMock.Id_Indexer_Set);
		Interactions.EnsureEvent(OptimizedDAllMembersMock.Id_SomeEvent_Subscribe);
		Interactions.EnsureEvent(OptimizedDAllMembersMock.Id_SomeEvent_Unsubscribe);

		Verify = new OptimizedDAllMembersVerifyFacade(Interactions);
	}

	public MockBehavior Behavior { get; } = MockBehavior.Default;
	public FastAllMembersInteractions Interactions { get; }
	public OptimizedDAllMembersVerifyFacade Verify { get; }
}

// ---- Verify facade — one assertion surface per member kind, each walks only its own buffer ----
public sealed class OptimizedDAllMembersVerifyFacade
{
	private readonly FastAllMembersInteractions _interactions;

	internal OptimizedDAllMembersVerifyFacade(FastAllMembersInteractions interactions)
	{
		_interactions = interactions;
	}

	public FastCountAssert2<int, string> MyFunc(IParameter<int> value, IParameter<string> name)
		=> new(_interactions.GetMethod2Buffer<int, string>(OptimizedDAllMembersMock.Id_MyFunc),
			OptimizedDAllMembersMock.Name_MyFunc,
			(IParameterMatch<int>)value, (IParameterMatch<string>)name);

	public PropertyVerify Counter => new(this);

	public IndexerVerify this[IParameter<string> key] => new(this, (IParameterMatch<string>)key);

	public EventVerify SomeEvent => new(
		_interactions.GetEventBuffer(OptimizedDAllMembersMock.Id_SomeEvent_Subscribe),
		_interactions.GetEventBuffer(OptimizedDAllMembersMock.Id_SomeEvent_Unsubscribe),
		OptimizedDAllMembersMock.Name_SomeEvent);

	public readonly struct PropertyVerify
	{
		private readonly OptimizedDAllMembersVerifyFacade _facade;
		internal PropertyVerify(OptimizedDAllMembersVerifyFacade facade) => _facade = facade;

		public FastCountAssert0 Got() => new(
			_facade._interactions.GetGetBuffer(OptimizedDAllMembersMock.Id_Counter_Get),
			OptimizedDAllMembersMock.Name_Counter, " (get)");

		public FastCountAssert1<int> Set(IParameter<int> value) => new(
			_facade._interactions.GetSetBuffer<int>(OptimizedDAllMembersMock.Id_Counter_Set),
			OptimizedDAllMembersMock.Name_Counter, " (set)",
			(IParameterMatch<int>)value);
	}

	public readonly struct IndexerVerify
	{
		private readonly OptimizedDAllMembersVerifyFacade _facade;
		private readonly IParameterMatch<string> _keyMatch;

		internal IndexerVerify(OptimizedDAllMembersVerifyFacade facade, IParameterMatch<string> keyMatch)
		{
			_facade = facade;
			_keyMatch = keyMatch;
		}

		public FastCountAssert1<string> Got() => new(
			_facade._interactions.GetIndexerGetBuffer<string>(OptimizedDAllMembersMock.Id_Indexer_Get),
			OptimizedDAllMembersMock.Name_Indexer, " (get)", _keyMatch);

		public FastCountAssert2<string, bool> Set(IParameter<bool> value) => new(
			_facade._interactions.GetMethod2Buffer<string, bool>(OptimizedDAllMembersMock.Id_Indexer_Set),
			OptimizedDAllMembersMock.Name_Indexer + " (set)",
			_keyMatch, (IParameterMatch<bool>)value);
	}

	public readonly struct EventVerify
	{
		private readonly FastEventBuffer _subscribe;
		private readonly FastEventBuffer _unsubscribe;
		private readonly string _name;

		internal EventVerify(FastEventBuffer subscribe, FastEventBuffer unsubscribe, string name)
		{
			_subscribe = subscribe;
			_unsubscribe = unsubscribe;
			_name = name;
		}

		public FastEventAssert Subscribed() => new(_subscribe, _name, " (subscribe)");
		public FastEventAssert Unsubscribed() => new(_unsubscribe, _name, " (unsubscribe)");
	}
}

// ---- Terminators — each walks exactly one per-member buffer. Same shape as FastCountAssert2 ----
public sealed class FastCountAssert0
{
	private readonly FastGetBuffer _buffer;
	private readonly string _name;
	private readonly string _kind;

	internal FastCountAssert0(FastGetBuffer buffer, string name, string kind)
	{
		_buffer = buffer;
		_name = name;
		_kind = kind;
	}

	public void Exactly(int times)
	{
		int found = _buffer.Count;
		if (found != times)
		{
			throw new MockVerificationException(
				$"Expected mock {_name}{_kind} exactly {times} time(s), but got {found}.");
		}
	}

	public void Once() => Exactly(1);
	public void Never() => Exactly(0);
}

public sealed class FastCountAssert1<T1>
{
	private readonly IFastCountableBuffer1<T1> _buffer;
	private readonly string _name;
	private readonly string _kind;
	private readonly IParameterMatch<T1> _m1;

	internal FastCountAssert1(IFastCountableBuffer1<T1> buffer, string name, string kind, IParameterMatch<T1> m1)
	{
		_buffer = buffer;
		_name = name;
		_kind = kind;
		_m1 = m1;
	}

	public void Exactly(int times)
	{
		int found = _buffer.CountMatching(_m1);
		if (found != times)
		{
			throw new MockVerificationException(
				$"Expected mock {_name}{_kind} exactly {times} time(s), but got {found}.");
		}
	}

	public void Once() => Exactly(1);
	public void Never() => Exactly(0);
}

public sealed class FastEventAssert
{
	private readonly FastEventBuffer _buffer;
	private readonly string _name;
	private readonly string _kind;

	internal FastEventAssert(FastEventBuffer buffer, string name, string kind)
	{
		_buffer = buffer;
		_name = name;
		_kind = kind;
	}

	public void Exactly(int times)
	{
		int found = _buffer.Count;
		if (found != times)
		{
			throw new MockVerificationException(
				$"Expected mock {_name}{_kind} exactly {times} time(s), but got {found}.");
		}
	}

	public void Once() => Exactly(1);
	public void Never() => Exactly(0);
}

internal interface IFastCountableBuffer1<T1>
{
	int CountMatching(IParameterMatch<T1> m1);
}

// ---- The store ----
public sealed class FastAllMembersInteractions : FastMockInteractions
{
	public FastAllMembersInteractions(int memberCount) : base(memberCount, skipInteractionRecording: false) { }

	// The new method-level buffer from the original D sketch is already reusable via
	// EnsureMemberBuffer<T1, T2>; here we add per-kind factory helpers so each member-kind gets
	// exactly the buffer shape it needs.
	public FastMethod2Buffer<T1, T2> EnsureMethod2<T1, T2>(int memberId)
	{
		FastMethod2Buffer<T1, T2> buf = new(this, memberId);
		InstallBuffer(memberId, buf);
		return buf;
	}

	public FastGetBuffer EnsurePropertyGet(int memberId)
	{
		FastGetBuffer buf = new(this, PropertyKind.Get);
		InstallBuffer(memberId, buf);
		return buf;
	}

	public FastSetBuffer<T> EnsurePropertySet<T>(int memberId)
	{
		FastSetBuffer<T> buf = new(this, PropertyKind.Set);
		InstallBuffer(memberId, buf);
		return buf;
	}

	public FastIndexerGetBuffer<T1> EnsureIndexerGet<T1>(int memberId)
	{
		FastIndexerGetBuffer<T1> buf = new(this);
		InstallBuffer(memberId, buf);
		return buf;
	}

	// Indexer set records (key, value) — reuse the 2-arg shape, just box into IndexerSetterAccess
	// at enumeration time.
	public FastMethod2Buffer<TKey, TValue> EnsureIndexerSet<TKey, TValue>(int memberId)
	{
		FastMethod2Buffer<TKey, TValue> buf = new(this, memberId, isIndexerSet: true);
		InstallBuffer(memberId, buf);
		return buf;
	}

	public FastEventBuffer EnsureEvent(int memberId)
	{
		FastEventBuffer buf = new(this, memberId);
		InstallBuffer(memberId, buf);
		return buf;
	}

	// ---- Hot-path Record* — one overload per member-kind × typed shape. The generator emits
	// a call site of the matching overload based on the declared member signature. ----
	public void RecordMethod2<T1, T2>(int memberId, string name, T1 v1, T2 v2)
		=> ((FastMethod2Buffer<T1, T2>)Buffers[memberId]!).Append(name, v1, v2);

	public void RecordPropertyGet(int memberId, string name)
		=> ((FastGetBuffer)Buffers[memberId]!).Append(name);

	public void RecordPropertySet<T>(int memberId, string name, T value)
		=> ((FastSetBuffer<T>)Buffers[memberId]!).Append(name, value);

	public void RecordIndexerGet<T1>(int memberId, string name, T1 key)
		=> ((FastIndexerGetBuffer<T1>)Buffers[memberId]!).Append(name, key);

	public void RecordIndexerSet<TKey, TValue>(int memberId, string name, TKey key, TValue value)
		=> ((FastMethod2Buffer<TKey, TValue>)Buffers[memberId]!).Append(name, key, value);

	public void RecordEvent(int memberId, string name, object? target, MethodInfo method)
		=> ((FastEventBuffer)Buffers[memberId]!).Append(name, target, method);

	// ---- Typed accessors used from the Verify facade ----
	public FastMethod2Buffer<T1, T2> GetMethod2Buffer<T1, T2>(int memberId) => (FastMethod2Buffer<T1, T2>)Buffers[memberId]!;
	public FastGetBuffer GetGetBuffer(int memberId) => (FastGetBuffer)Buffers[memberId]!;
	public FastSetBuffer<T> GetSetBuffer<T>(int memberId) => (FastSetBuffer<T>)Buffers[memberId]!;
	public FastIndexerGetBuffer<T1> GetIndexerGetBuffer<T1>(int memberId) => (FastIndexerGetBuffer<T1>)Buffers[memberId]!;
	public FastEventBuffer GetEventBuffer(int memberId) => (FastEventBuffer)Buffers[memberId]!;
}

// ---- Per-kind buffers (struct records, lock only on resize, boxing only on enumeration) ----

public enum PropertyKind { Get, Set, }

/// <summary>Buffer for 2-arg methods. Also reused for indexer setters (key, value).</summary>
public sealed class FastMethod2Buffer<T1, T2> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
	private readonly int _memberId;
	private readonly bool _isIndexerSet;
	private SlimCall2<T1, T2>[] _records;
	private int _count;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

	internal FastMethod2Buffer(FastMockInteractions owner, int memberId, bool isIndexerSet = false)
	{
		_owner = owner;
		_memberId = memberId;
		_isIndexerSet = isIndexerSet;
		_records = new SlimCall2<T1, T2>[4];
	}

	public int Count => Volatile.Read(ref _count);

	public void Append(string name, T1 v1, T2 v2)
	{
		long seq = _owner.NextSequence();
		lock (_lock)
		{
			int n = _count;
			if (n == _records.Length)
			{
				Array.Resize(ref _records, n * 2);
			}

			_records[n] = new SlimCall2<T1, T2> { Seq = seq, MethodName = name, P1 = v1, P2 = v2, };
			Volatile.Write(ref _count, n + 1);
		}

		_owner.RaiseAdded();
	}

	public int CountMatching(IParameterMatch<T1> m1, IParameterMatch<T2> m2)
	{
		SlimCall2<T1, T2>[] snap;
		int n;
		lock (_lock)
		{
			n = _count;
			snap = _records;
		}

		int hit = 0;
		for (int i = 0; i < n; i++)
		{
			ref SlimCall2<T1, T2> r = ref snap[i];
			if (m1.Matches(r.P1) && m2.Matches(r.P2))
			{
				hit++;
			}
		}

		return hit;
	}

	public void Clear()
	{
		lock (_lock) { _count = 0; }
	}

	void IFastMemberBuffer.AppendBoxed(List<(long seq, IInteraction i)> dest)
	{
		SlimCall2<T1, T2>[] snap;
		int n;
		lock (_lock)
		{
			n = _count;
			snap = _records;
		}

		for (int i = 0; i < n; i++)
		{
			ref SlimCall2<T1, T2> r = ref snap[i];
			IInteraction boxed = _isIndexerSet
				? (IInteraction)new IndexerSetterAccess<T1, T2>("key", r.P1, r.P2)  // param name only used for display
				: new SlimMethodInvocation<T1, T2>(r.MethodName, r.P1, r.P2);
			dest.Add((r.Seq, boxed));
		}
	}
}

/// <summary>Buffer for property getters — no payload beyond the seq and the property name.</summary>
public sealed class FastGetBuffer : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
	private readonly PropertyKind _kind;
	private (long Seq, string Name)[] _records = new (long, string)[4];
	private int _count;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

	internal FastGetBuffer(FastMockInteractions owner, PropertyKind kind)
	{
		_owner = owner;
		_kind = kind;
	}

	public int Count => Volatile.Read(ref _count);

	public void Append(string name)
	{
		long seq = _owner.NextSequence();
		lock (_lock)
		{
			int n = _count;
			if (n == _records.Length)
			{
				Array.Resize(ref _records, n * 2);
			}

			_records[n] = (seq, name);
			Volatile.Write(ref _count, n + 1);
		}

		_owner.RaiseAdded();
	}

	public void Clear() { lock (_lock) { _count = 0; } }

	void IFastMemberBuffer.AppendBoxed(List<(long seq, IInteraction i)> dest)
	{
		(long, string)[] snap;
		int n;
		lock (_lock)
		{
			n = _count;
			snap = _records;
		}

		for (int i = 0; i < n; i++)
		{
			(long seq, string name) r = snap[i];
			dest.Add((r.seq, new PropertyGetterAccess(r.name)));
		}
	}
}

/// <summary>Buffer for property setters — stores the assigned value. Enumerates as <c>PropertySetterAccess&lt;T&gt;</c>.</summary>
public sealed class FastSetBuffer<T> : IFastMemberBuffer, IFastCountableBuffer1<T>
{
	private readonly FastMockInteractions _owner;
	private readonly PropertyKind _kind;
	private (long Seq, string Name, T Value)[] _records = new (long, string, T)[4];
	private int _count;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

	internal FastSetBuffer(FastMockInteractions owner, PropertyKind kind)
	{
		_owner = owner;
		_kind = kind;
	}

	public int Count => Volatile.Read(ref _count);

	public void Append(string name, T value)
	{
		long seq = _owner.NextSequence();
		lock (_lock)
		{
			int n = _count;
			if (n == _records.Length)
			{
				Array.Resize(ref _records, n * 2);
			}

			_records[n] = (seq, name, value);
			Volatile.Write(ref _count, n + 1);
		}

		_owner.RaiseAdded();
	}

	public int CountMatching(IParameterMatch<T> m1)
	{
		(long, string, T)[] snap;
		int n;
		lock (_lock)
		{
			n = _count;
			snap = _records;
		}

		int hit = 0;
		for (int i = 0; i < n; i++)
		{
			if (m1.Matches(snap[i].Item3))
			{
				hit++;
			}
		}

		return hit;
	}

	public void Clear() { lock (_lock) { _count = 0; } }

	void IFastMemberBuffer.AppendBoxed(List<(long seq, IInteraction i)> dest)
	{
		(long, string, T)[] snap;
		int n;
		lock (_lock)
		{
			n = _count;
			snap = _records;
		}

		for (int i = 0; i < n; i++)
		{
			(long seq, string name, T value) r = snap[i];
			dest.Add((r.seq, new PropertySetterAccess<T>(r.name, r.value)));
		}
	}
}

/// <summary>Buffer for indexer getters — stores the single key. Enumerates as <c>IndexerGetterAccess&lt;T1&gt;</c>.</summary>
public sealed class FastIndexerGetBuffer<T1> : IFastMemberBuffer, IFastCountableBuffer1<T1>
{
	private readonly FastMockInteractions _owner;
	private (long Seq, string Name, T1 Key)[] _records = new (long, string, T1)[4];
	private int _count;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

	internal FastIndexerGetBuffer(FastMockInteractions owner)
	{
		_owner = owner;
	}

	public int Count => Volatile.Read(ref _count);

	public void Append(string name, T1 key)
	{
		long seq = _owner.NextSequence();
		lock (_lock)
		{
			int n = _count;
			if (n == _records.Length)
			{
				Array.Resize(ref _records, n * 2);
			}

			_records[n] = (seq, name, key);
			Volatile.Write(ref _count, n + 1);
		}

		_owner.RaiseAdded();
	}

	public int CountMatching(IParameterMatch<T1> m1)
	{
		(long, string, T1)[] snap;
		int n;
		lock (_lock)
		{
			n = _count;
			snap = _records;
		}

		int hit = 0;
		for (int i = 0; i < n; i++)
		{
			if (m1.Matches(snap[i].Item3))
			{
				hit++;
			}
		}

		return hit;
	}

	public void Clear() { lock (_lock) { _count = 0; } }

	void IFastMemberBuffer.AppendBoxed(List<(long seq, IInteraction i)> dest)
	{
		(long, string, T1)[] snap;
		int n;
		lock (_lock)
		{
			n = _count;
			snap = _records;
		}

		for (int i = 0; i < n; i++)
		{
			(long seq, string name, T1 key) r = snap[i];
			dest.Add((r.seq, new IndexerGetterAccess<T1>("key", r.key)));
		}
	}
}

/// <summary>Buffer for event subscribe or unsubscribe — enumerates as <c>EventSubscription</c> / <c>EventUnsubscription</c>.</summary>
public sealed class FastEventBuffer : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
	private readonly int _memberId;
	private (long Seq, string Name, object? Target, MethodInfo Method)[] _records
		= new (long, string, object?, MethodInfo)[4];
	private int _count;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

	internal FastEventBuffer(FastMockInteractions owner, int memberId)
	{
		_owner = owner;
		_memberId = memberId;
	}

	public int Count => Volatile.Read(ref _count);

	public void Append(string name, object? target, MethodInfo method)
	{
		long seq = _owner.NextSequence();
		lock (_lock)
		{
			int n = _count;
			if (n == _records.Length)
			{
				Array.Resize(ref _records, n * 2);
			}

			_records[n] = (seq, name, target, method);
			Volatile.Write(ref _count, n + 1);
		}

		_owner.RaiseAdded();
	}

	public void Clear() { lock (_lock) { _count = 0; } }

	void IFastMemberBuffer.AppendBoxed(List<(long seq, IInteraction i)> dest)
	{
		(long, string, object?, MethodInfo)[] snap;
		int n;
		lock (_lock)
		{
			n = _count;
			snap = _records;
		}

		bool isSubscribe = _memberId == OptimizedDAllMembersMock.Id_SomeEvent_Subscribe;
		for (int i = 0; i < n; i++)
		{
			(long seq, string name, object? target, MethodInfo method) r = snap[i];
			IInteraction boxed = isSubscribe
				? (IInteraction)new EventSubscription(r.name, r.target, r.method)
				: new EventUnsubscription(r.name, r.target, r.method);
			dest.Add((r.seq, boxed));
		}
	}
}

// ---- Smoke test across all member kinds — proves Verify, Monitor, and ordered enumeration
// all behave correctly for every member-kind buffer. ----
public static class OptimizedDAllMembersSmokeTest
{
	public static void Run()
	{
		OptimizedDAllMembersMock mock = new();

		// Monitor captures from seq 0.
		FastMockMonitor monitor = new(mock.Mock.Interactions, OptimizedDAllMembersMock.MemberCount);
		using (monitor.Run())
		{
			// Exercise all six member kinds in a mixed order so the seq ordering is non-trivial.
			mock.MyFunc(1, "a");
			mock.Counter = 7;
			int _ = mock.Counter;
			mock["k"] = true;
			bool __ = mock["k"];
			EventHandler handler = (_, _) => { };
			mock.SomeEvent += handler;
			mock.SomeEvent -= handler;
		}

		// Count via per-member buffers (Verify path).
		mock.Mock.Verify.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Exactly(1);
		mock.Mock.Verify.Counter.Got().Once();
		mock.Mock.Verify.Counter.Set(It.IsAny<int>()).Once();
		mock.Mock.Verify.Counter.Set(It.Is(7)).Once();
		mock.Mock.Verify.Counter.Set(It.Is(8)).Never();
		mock.Mock.Verify[It.IsAny<string>()].Got().Once();
		mock.Mock.Verify[It.Is("k")].Got().Once();
		mock.Mock.Verify[It.Is("z")].Got().Never();
		mock.Mock.Verify[It.IsAny<string>()].Set(It.IsAny<bool>()).Once();
		mock.Mock.Verify.SomeEvent.Subscribed().Once();
		mock.Mock.Verify.SomeEvent.Unsubscribed().Once();

		// Enumerated total = all 7 interactions, in monotonic seq order.
		IInteraction[] all = mock.Mock.Interactions.ToArray();
		Require(all.Length == 7, "ordered enumeration yields 7 interactions");
		Require(all[0] is SlimMethodInvocation<int, string>, "order[0] = method");
		Require(all[1] is PropertySetterAccess<int>, "order[1] = Counter set");
		Require(all[2] is PropertyGetterAccess, "order[2] = Counter get");
		Require(all[3] is IndexerSetterAccess<string, bool>, "order[3] = indexer set");
		Require(all[4] is IndexerGetterAccess<string>, "order[4] = indexer get");
		Require(all[5] is EventSubscription, "order[5] = event subscribe");
		Require(all[6] is EventUnsubscription, "order[6] = event unsubscribe");

		// Monitor captured the same 7 in the same order.
		Require(monitor.Interactions.Count == 7, "monitor captured all 7");
		IInteraction[] monitored = monitor.Interactions.ToArray();
		Require(monitored.Length == 7 && monitored[6] is EventUnsubscription,
			"monitor order matches source");

		mock.Mock.Interactions.Clear();
		Require(mock.Mock.Interactions.Count == 0, "clear resets count for all buffers");
	}

	private static void Require(bool condition, string description)
	{
		if (!condition)
		{
			throw new InvalidOperationException($"AllMembers smoke test failed: {description}");
		}
	}
}
