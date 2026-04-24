// PREVIEW — D-refactor reference harness (Phase 0). This file is hand-written to model the
// optimized mock shape the runtime library and source generator will adopt across Phases 1–6.
// It will be deleted in Step 7.3 once the generator emits the real thing. Do not depend on
// any type defined here from outside the benchmark project.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mockolate;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Benchmarks.Optimized;

/// <summary>
///     "D" refactor sketch: replaces the shared <see cref="MockInteractions" /> list with per-member
///     typed struct buffers. Applies A + B + C + D + E + F without dropping features.
///     <para />
///     What's different from <see cref="OptimizedMy2ParamMock" />:
///     <list type="bullet">
///       <item><description>
///         Interactions are appended to a per-member <see cref="FastMemberBuffer2{T1,T2}" />, never
///         to a shared list. The hot path allocates only when the per-member buffer has to grow —
///         amortized zero bytes per call.
///       </description></item>
///       <item><description>
///         Each record gets a monotonic <see cref="long" /> sequence number. Global enumeration
///         merges all buffers in seq order, so Monitor and <c>GetUnverifiedInteractions</c> keep
///         a stable view.
///       </description></item>
///       <item><description>
///         Verify counts walk only the relevant member's buffer. Matchers receive the typed
///         values directly — no boxing, no parameter-name strings, no closure per interaction.
///       </description></item>
///       <item><description>
///         A <see cref="FastMockMonitor{T}" /> alternative proves the Monitor contract still works:
///         global count + skip-from-start + <c>InteractionAdded</c>/<c>OnClearing</c> events +
///         <c>Clear</c>. Boxing of struct records into <see cref="IInteraction" /> instances happens
///         only when the monitor actually copies its scope — never on the hot path.
///       </description></item>
///     </list>
///     Feature parity notes: to land this upstream the library would need to turn
///     <see cref="MockInteractions" />' methods virtual (or introduce an interface) so
///     <see cref="Mockolate.Verify.VerificationResult{TVerify}" /> and
///     <see cref="Mockolate.Monitor.MockMonitor" /> can delegate to the per-member implementation.
///     That surgery is the whole point of this "larger refactoring" sketch.
/// </summary>
public sealed class OptimizedDMy2ParamMock : IMy2ParamInterface
{
	internal const int MemberId_MyFunc = 0;
	internal const int MemberCount = 1;

	private const string MethodName_MyFunc =
		"Mockolate.Benchmarks.Optimized.IMy2ParamInterface.MyFunc";

	private readonly FastReturnMethodSetup<bool, int, string>[]?[] _setupsByMemberId =
		new FastReturnMethodSetup<bool, int, string>[MemberCount][];
#if NET10_0_OR_GREATER
	private readonly Lock _setupLock = new();
#else
	private readonly object _setupLock = new();
#endif

	public OptimizedDMy2ParamMock() : this(MockBehavior.Default)
	{
	}

	public OptimizedDMy2ParamMock(MockBehavior behavior)
	{
		Mock = new OptimizedDMockFacade(this, behavior);
	}

	public OptimizedDMockFacade Mock { get; }

	// ---- HOT PATH ----
	public bool MyFunc(int value, string name)
	{
		// (A)+(B)+(C) volatile snapshot + typed fast match, no closure.
		FastReturnMethodSetup<bool, int, string>? matched = null;
		FastReturnMethodSetup<bool, int, string>[]? snapshot =
			Volatile.Read(ref _setupsByMemberId[MemberId_MyFunc]);
		if (snapshot is not null)
		{
			for (int i = snapshot.Length - 1; i >= 0; i--)
			{
				FastReturnMethodSetup<bool, int, string> s = snapshot[i];
				if (s.MatchesFast(value, name))
				{
					matched = s;
					break;
				}
			}
		}

		MockBehavior behavior = Mock.Behavior;

		// (D) per-member typed buffer — no shared lock, no boxing, amortized 0 alloc.
		if (!behavior.SkipInteractionRecording)
		{
			Mock.FastInteractions.RecordMyFunc(MemberId_MyFunc, MethodName_MyFunc, value, name);
		}

		bool returnValue = default;
		bool hasReturnValue = false;
		try
		{
			if (matched is not null && matched.TryGetReturnValue(value, name, out bool v))
			{
				returnValue = v;
				hasReturnValue = true;
			}
		}
		finally
		{
			// (F) state-passing TriggerCallbacks — no closure.
			matched?.TriggerCallbacks(value, name);
		}

		if (matched is null && !hasReturnValue && behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException(
				$"The method '{MethodName_MyFunc}(int, string)' was invoked without prior setup.");
		}

		if (hasReturnValue)
		{
			return returnValue;
		}

		return behavior.DefaultValue.GenerateValue(typeof(bool)) is bool b ? b : default;
	}

	internal void AddSetup(int memberId, FastReturnMethodSetup<bool, int, string> setup)
	{
		lock (_setupLock)
		{
			FastReturnMethodSetup<bool, int, string>[]? existing = _setupsByMemberId[memberId];
			FastReturnMethodSetup<bool, int, string>[] next;
			if (existing is null)
			{
				next = new[] { setup, };
			}
			else
			{
				next = new FastReturnMethodSetup<bool, int, string>[existing.Length + 1];
				Array.Copy(existing, next, existing.Length);
				next[existing.Length] = setup;
			}

			Volatile.Write(ref _setupsByMemberId[memberId], next);
		}
	}
}

public sealed class OptimizedDMockFacade
{
	internal OptimizedDMockFacade(OptimizedDMy2ParamMock owner, MockBehavior behavior)
	{
		Owner = owner;
		Behavior = behavior;
		FastInteractions = new FastMockInteractions(OptimizedDMy2ParamMock.MemberCount, behavior.SkipInteractionRecording);
		FastInteractions.EnsureMemberBuffer<int, string>(OptimizedDMy2ParamMock.MemberId_MyFunc);

		Setup = new OptimizedDSetupFacade(owner);
		Verify = new OptimizedDVerifyFacade(this);
	}

	internal OptimizedDMy2ParamMock Owner { get; }
	public MockBehavior Behavior { get; }
	public FastMockInteractions FastInteractions { get; }
	public OptimizedDSetupFacade Setup { get; }
	public OptimizedDVerifyFacade Verify { get; }
}

public sealed class OptimizedDSetupFacade
{
	private readonly OptimizedDMy2ParamMock _owner;

	internal OptimizedDSetupFacade(OptimizedDMy2ParamMock owner)
	{
		_owner = owner;
	}

	public FastReturnMethodSetup<bool, int, string> MyFunc(
		IParameter<int> value, IParameter<string> name)
	{
		// We need a MockRegistry for ReturnMethodSetup's base ctor; a one-off throwaway registry is
		// enough for this benchmark since setup-time MockRegistry access isn't exercised.
		FastReturnMethodSetup<bool, int, string> setup =
			new(new MockRegistry(_owner.Mock.Behavior),
				"Mockolate.Benchmarks.Optimized.IMy2ParamInterface.MyFunc",
				(IParameterMatch<int>)value, (IParameterMatch<string>)name);
		_owner.AddSetup(OptimizedDMy2ParamMock.MemberId_MyFunc, setup);
		return setup;
	}
}

public sealed class OptimizedDVerifyFacade
{
	private readonly OptimizedDMockFacade _mock;

	internal OptimizedDVerifyFacade(OptimizedDMockFacade mock)
	{
		_mock = mock;
	}

	public FastCountAssert2<int, string> MyFunc(IParameter<int> value, IParameter<string> name)
	{
		return new FastCountAssert2<int, string>(
			_mock.FastInteractions,
			OptimizedDMy2ParamMock.MemberId_MyFunc,
			"Mockolate.Benchmarks.Optimized.IMy2ParamInterface.MyFunc",
			(IParameterMatch<int>)value, (IParameterMatch<string>)name);
	}
}

/// <summary>
///     Terminating assertion for a verification against a 2-parameter member. Walks the relevant
///     per-member buffer once, tallies matches, then throws on mismatch — the full
///     <c>Exactly</c>/<c>Once</c>/<c>Never</c>/<c>AtLeast</c>/<c>AtMost</c>/<c>Between</c>/<c>Times</c>
///     surface from <c>VerificationResultExtensions</c> maps directly onto the same pattern.
/// </summary>
public sealed class FastCountAssert2<T1, T2>
{
	private readonly FastMockInteractions? _interactions;
	private readonly int _memberId;
	private readonly FastMethod2Buffer<T1, T2>? _directBuffer;
	private readonly string _methodName;
	private IParameterMatch<T1> _m1;
	private IParameterMatch<T2> _m2;
	private bool _ignoreParameters;

	internal FastCountAssert2(FastMockInteractions interactions, int memberId,
		string methodName, IParameterMatch<T1> m1, IParameterMatch<T2> m2)
	{
		_interactions = interactions;
		_memberId = memberId;
		_methodName = methodName;
		_m1 = m1;
		_m2 = m2;
	}

	internal FastCountAssert2(FastMethod2Buffer<T1, T2> buffer, string methodName,
		IParameterMatch<T1> m1, IParameterMatch<T2> m2)
	{
		_directBuffer = buffer;
		_methodName = methodName;
		_m1 = m1;
		_m2 = m2;
	}

	public FastCountAssert2<T1, T2> AnyParameters()
	{
		_ignoreParameters = true;
		return this;
	}

	public void Exactly(int times)
	{
		int found = Count();
		if (found != times)
		{
			throw new MockVerificationException(
				$"Expected that mock invoked {_methodName}(...) exactly {times} time(s), but it was {found}.");
		}
	}

	public void Once() => Exactly(1);
	public void Never() => Exactly(0);
	public void AtLeast(int times)
	{
		int found = Count();
		if (found < times)
		{
			throw new MockVerificationException(
				$"Expected that mock invoked {_methodName}(...) at least {times} time(s), but it was {found}.");
		}
	}

	private int Count()
	{
		if (_directBuffer is not null)
		{
			return _ignoreParameters ? _directBuffer.Count : _directBuffer.CountMatching(_m1, _m2);
		}

		FastMemberBuffer2<T1, T2> buffer = _interactions!.GetMemberBuffer<T1, T2>(_memberId);
		return _ignoreParameters ? buffer.Count : buffer.CountMatching(_m1, _m2);
	}
}

/// <summary>
///     Per-mock interaction storage that mirrors the public surface of <see cref="MockInteractions" />
///     (<c>Count</c>, enumeration, <c>InteractionAdded</c>/<c>OnClearing</c> events, <c>Clear</c>,
///     <c>GetUnverifiedInteractions</c>) but backs it with typed per-member buffers.
///     <para />
///     Enumeration returns interactions in monotonic <c>seq</c> order, so <c>Skip(n)</c> semantics
///     used by <see cref="Mockolate.Monitor.MockMonitor" /> remain intact.
/// </summary>
public class FastMockInteractions : IReadOnlyCollection<IInteraction>
{
	protected readonly IFastMemberBuffer?[] Buffers;
	private long _globalSequence;

#if NET10_0_OR_GREATER
	private readonly Lock _verifiedLock = new();
#else
	private readonly object _verifiedLock = new();
#endif

	private HashSet<IInteraction>? _verified;

	public FastMockInteractions(int memberCount, bool skipInteractionRecording)
	{
		Buffers = new IFastMemberBuffer?[memberCount];
		SkipInteractionRecording = skipInteractionRecording;
	}

	public bool SkipInteractionRecording { get; }

	/// <inheritdoc cref="MockInteractions.Count" />
	public int Count => (int)Interlocked.Read(ref _globalSequence);

	/// <inheritdoc cref="Mockolate.Interactions.MockInteractions.InteractionAdded" />
	public event EventHandler? InteractionAdded;
	public event EventHandler? OnClearing;

	internal long NextSequence() => Interlocked.Increment(ref _globalSequence) - 1;
	internal void RaiseAdded() => InteractionAdded?.Invoke(this, EventArgs.Empty);

	public FastMemberBuffer2<T1, T2> EnsureMemberBuffer<T1, T2>(int memberId)
	{
		IFastMemberBuffer? existing = Buffers[memberId];
		if (existing is FastMemberBuffer2<T1, T2> typed)
		{
			return typed;
		}

		FastMemberBuffer2<T1, T2> created = new(this, memberId);
		Buffers[memberId] = created;
		return created;
	}

	/// <summary>
	///     Lets derived stores install their own <see cref="IFastMemberBuffer" /> implementations
	///     (needed for the richer per-kind buffers used by <see cref="FastAllMembersInteractions" />).
	/// </summary>
	protected void InstallBuffer(int memberId, IFastMemberBuffer buffer)
	{
		Buffers[memberId] = buffer;
	}

	public FastMemberBuffer2<T1, T2> GetMemberBuffer<T1, T2>(int memberId)
		=> (FastMemberBuffer2<T1, T2>)Buffers[memberId]!;

	public void RecordMyFunc(int memberId, string name, int v, string s)
		=> ((FastMemberBuffer2<int, string>)Buffers[memberId]!).Append(name, v, s);

	public void Clear()
	{
		Interlocked.Exchange(ref _globalSequence, 0);
		foreach (IFastMemberBuffer? b in Buffers)
		{
			b?.Clear();
		}

		lock (_verifiedLock)
		{
			_verified = null;
		}

		OnClearing?.Invoke(this, EventArgs.Empty);
	}

	public IReadOnlyCollection<IInteraction> GetUnverifiedInteractions()
	{
		IInteraction[] snapshot = SnapshotOrdered();
		lock (_verifiedLock)
		{
			if (_verified is null || _verified.Count == 0)
			{
				return snapshot;
			}

			List<IInteraction> result = new(snapshot.Length);
			foreach (IInteraction i in snapshot)
			{
				if (!_verified.Contains(i))
				{
					result.Add(i);
				}
			}

			return result;
		}
	}

	internal void Verified(IEnumerable<IInteraction> interactions)
	{
		lock (_verifiedLock)
		{
			_verified ??= new HashSet<IInteraction>();
			foreach (IInteraction i in interactions)
			{
				_verified.Add(i);
			}
		}
	}

	public IEnumerator<IInteraction> GetEnumerator()
		=> ((IEnumerable<IInteraction>)SnapshotOrdered()).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	private IInteraction[] SnapshotOrdered()
	{
		// Snapshot each buffer, then merge in monotonic seq order. Boxing into
		// SlimMethodInvocation happens here only — never on the hot path.
		List<(long seq, IInteraction i)> all = new();
		foreach (IFastMemberBuffer? b in Buffers)
		{
			b?.AppendBoxed(all);
		}

		all.Sort(static (a, b) => a.seq.CompareTo(b.seq));
		IInteraction[] result = new IInteraction[all.Count];
		for (int i = 0; i < all.Count; i++)
		{
			result[i] = all[i].i;
		}

		return result;
	}
}

public interface IFastMemberBuffer
{
	void Clear();
	void AppendBoxed(List<(long seq, IInteraction i)> dest);
}

/// <summary>
///     Per-member buffer for 2-parameter methods. Stores struct records (<see cref="SlimCall2{T1,T2}" />)
///     in a grow-on-demand array, protected by a per-buffer lock (only contended on resize).
///     The hot <see cref="Append" /> path bumps a sequence counter, writes into the slot, and fires
///     <see cref="FastMockInteractions.InteractionAdded" /> — no shared lock with other members.
/// </summary>
public sealed class FastMemberBuffer2<T1, T2> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
	private readonly int _memberId;
	private SlimCall2<T1, T2>[] _records;
	private int _count;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

	internal FastMemberBuffer2(FastMockInteractions owner, int memberId)
	{
		_owner = owner;
		_memberId = memberId;
		_records = new SlimCall2<T1, T2>[4];
	}

	public int Count => Volatile.Read(ref _count);

	public void Append(string methodName, T1 p1, T2 p2)
	{
		long seq = _owner.NextSequence();
		lock (_lock)
		{
			int n = _count;
			if (n == _records.Length)
			{
				Array.Resize(ref _records, n * 2);
			}

			_records[n].Seq = seq;
			_records[n].MethodName = methodName;
			_records[n].P1 = p1;
			_records[n].P2 = p2;
			Volatile.Write(ref _count, n + 1);
		}

		_owner.RaiseAdded();
	}

	public int CountMatching(IParameterMatch<T1> m1, IParameterMatch<T2> m2)
	{
		SlimCall2<T1, T2>[] snapshot;
		int n;
		lock (_lock)
		{
			n = _count;
			snapshot = _records;
		}

		int hit = 0;
		for (int i = 0; i < n; i++)
		{
			ref SlimCall2<T1, T2> r = ref snapshot[i];
			if (m1.Matches(r.P1) && m2.Matches(r.P2))
			{
				hit++;
			}
		}

		return hit;
	}

	public void Clear()
	{
		lock (_lock)
		{
			_count = 0;
		}
	}

	void IFastMemberBuffer.AppendBoxed(List<(long seq, IInteraction i)> dest)
	{
		SlimCall2<T1, T2>[] snapshot;
		int n;
		lock (_lock)
		{
			n = _count;
			snapshot = _records;
		}

		for (int i = 0; i < n; i++)
		{
			ref SlimCall2<T1, T2> r = ref snapshot[i];
			dest.Add((r.Seq, new SlimMethodInvocation<T1, T2>(r.MethodName, r.P1, r.P2)));
		}
	}
}

/// <summary>
///     Struct record for a 2-parameter call. Fixed size, no boxing, carries a monotonic sequence
///     number so ordered enumeration works across per-member buffers.
/// </summary>
public struct SlimCall2<T1, T2>
{
	public long Seq;
	public string MethodName;
	public T1 P1;
	public T2 P2;
}

/// <summary>
///     Monitor for a <see cref="FastMockInteractions" /> — mirrors the semantics of
///     <see cref="Mockolate.Monitor.MockMonitor" />: begin a scope, capture the starting index,
///     copy any records produced during the scope into a read-only view when the scope ends.
///     <para />
///     The monitor's captured slice is type-agnostic — it copies whatever the source produced during
///     the scope (methods, property accesses, indexer accesses, event sub/unsub) via the source's
///     ordered enumeration. Because Monitor is a cold path used for offline inspection, a simple
///     list-backed view is enough — no per-member buffers needed inside the monitor's own store.
/// </summary>
public sealed class FastMockMonitor
{
	private readonly FastMockInteractions _source;
	private int _start = -1;
	private readonly MonitorView _view;

	public FastMockMonitor(FastMockInteractions source, int memberCount)
	{
		_source = source;
		_view = new MonitorView();
	}

	/// <summary>
	///     Read-only snapshot of interactions captured during the monitor scope, in source-order.
	///     Mirrors <see cref="Mockolate.Monitor.MockMonitor.Interactions" /> for consumers.
	/// </summary>
	public IReadOnlyCollection<IInteraction> Interactions => _view;

	public IDisposable Run()
	{
		if (_start >= 0)
		{
			throw new InvalidOperationException("Monitoring is already running.");
		}

		_start = _source.Count;
		_source.OnClearing += HandleClearing;
		return new Scope(Stop);
	}

	private void HandleClearing(object? sender, EventArgs e)
	{
		_start = 0;
		_view.Clear();
	}

	private void Stop()
	{
		if (_start >= 0)
		{
			// Take the ordered [_start..] slice of the source and stash it in the view — the
			// source already boxes per-member buffer struct records into IInteraction instances
			// during enumeration, so this preserves the full interaction type (methods, property
			// accesses, indexer accesses, event sub/unsub) with no monitor-specific dispatch.
			int i = 0;
			foreach (IInteraction interaction in _source)
			{
				if (i++ < _start)
				{
					continue;
				}

				_view.Add(interaction);
			}
		}

		_source.OnClearing -= HandleClearing;
		_start = -1;
	}

	private sealed class Scope : IDisposable
	{
		private readonly Action _onDispose;
		public Scope(Action onDispose) => _onDispose = onDispose;
		public void Dispose() => _onDispose();
	}

	private sealed class MonitorView : IReadOnlyCollection<IInteraction>
	{
		private readonly List<IInteraction> _captured = new();
		public int Count => _captured.Count;
		public IEnumerator<IInteraction> GetEnumerator() => _captured.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		internal void Add(IInteraction i) => _captured.Add(i);
		internal void Clear() => _captured.Clear();
	}
}

/// <summary>
///     Minimal smoke test that exercises the D-optimized mock through: fluent setup → 3 calls with
///     different arguments → Verify count (full + filtered) → Monitor scope → <c>Clear</c>. Not a
///     proper unit test, just enough to flag a regression in this sketch at process start.
/// </summary>
public static class OptimizedDMockSmokeTest
{
	public static void Run()
	{
		OptimizedDMy2ParamMock mock = new();
		((IReturnMethodSetup<bool, int, string>)mock.Mock.Setup.MyFunc(
			It.IsAny<int>(), It.IsAny<string>())).Returns(true);

		// Monitor captures starting at seq 0.
		FastMockMonitor monitor = new(mock.Mock.FastInteractions, OptimizedDMy2ParamMock.MemberCount);
		using (monitor.Run())
		{
			Require(mock.MyFunc(1, "a"), "first call returns true");
			Require(mock.MyFunc(2, "b"), "second call returns true");
			Require(mock.MyFunc(3, "a"), "third call returns true");
		}

		Require(mock.Mock.FastInteractions.Count == 3, "source has 3 interactions");
		Require(monitor.Interactions.Count == 3, "monitor captured all 3");

		mock.Mock.Verify.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Exactly(3);
		mock.Mock.Verify.MyFunc(It.IsAny<int>(), It.Is("a")).Exactly(2);
		mock.Mock.Verify.MyFunc(It.IsAny<int>(), It.Is("z")).Never();

		IInteraction[] ordered = mock.Mock.FastInteractions.ToArray();
		Require(ordered.Length == 3, "ordered enumeration yields 3");
		Require(((SlimMethodInvocation<int, string>)ordered[0]).Parameter1 == 1, "seq-order preserved");
		Require(((SlimMethodInvocation<int, string>)ordered[2]).Parameter1 == 3, "seq-order tail");

		mock.Mock.FastInteractions.Clear();
		Require(mock.Mock.FastInteractions.Count == 0, "clear resets count");
	}

	private static void Require(bool condition, string description)
	{
		if (!condition)
		{
			throw new InvalidOperationException($"OptimizedD smoke test failed: {description}");
		}
	}
}
