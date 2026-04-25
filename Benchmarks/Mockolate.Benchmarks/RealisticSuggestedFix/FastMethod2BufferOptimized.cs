// Demonstration buffer for the suggested D-refactor fix. Lives next to the existing
// FastMethod2Buffer<T1, T2> and is structurally identical except for three changes:
//
//   1. Lock-free Append: a slot is reserved via Interlocked.Increment(&_reserved) and the
//      backing array is only locked when it has to grow. Compared with the existing buffer's
//      per-Append `lock (_lock)`, this saves the lock-acquire/release on every recorded call.
//
//   2. Slim Record: drops `Name` and `Boxed` fields. The method name is already constant per
//      buffer instance (it's the same string captured by the proxy emit-site), so storing it
//      on every slot is dead weight. Boxed reference moves to a side-allocated dictionary
//      that's only populated when AppendBoxed runs (i.e. ordered enumeration / Monitor /
//      GetUnverifiedInteractions). Verify count terminators don't touch it.
//
//   3. Typed CountMatching: a public method that walks the typed Record[] directly with the
//      caller's matchers. Returns the count. The Verify-count path calls this and throws on
//      mismatch — never builds an IInteraction[] and never allocates MethodInvocation<T1,T2>
//      on the hot path. Compare with the current VerificationResult<T>.CollectMatching, which
//      always boxes every record into MethodInvocation<T1,T2> regardless of the terminator.
//
// To wire this into the actual runtime, the source generator's proxy-method body would change
// the cast from FastMethod2Buffer<T1,T2> to FastMethod2BufferOptimized<T1,T2> (one token), the
// generator's Verify-method emission would route count terminators through CountMatching
// (a few lines), and the existing FastMethod2Buffer<T1,T2> would either gain CountMatching as
// a virtual or be replaced wholesale.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Benchmarks.RealisticSuggestedFix;

[DebuggerDisplay("{Count} method calls (optimized)")]
public sealed class FastMethod2BufferOptimized<T1, T2> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
	private readonly string _methodName;
#if NET10_0_OR_GREATER
	private readonly Lock _growLock = new();
#else
	private readonly object _growLock = new();
#endif
	private SlimRecord[] _records;
	private int _reserved;
	private int _published;
	private Dictionary<int, MethodInvocation<T1, T2>>? _boxed;

	internal FastMethod2BufferOptimized(FastMockInteractions owner, string methodName)
	{
		_owner = owner;
		_methodName = methodName;
		_records = new SlimRecord[8];
	}

	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records a 2-parameter method call. Lock-free: Interlocked-reserves a slot, writes
	///     the typed parameter values, publishes the count. Only locks if the backing array
	///     needs to grow.
	/// </summary>
	public void Append(string name, T1 parameter1, T2 parameter2)
	{
		long seq = _owner.NextSequence();
		int i = Interlocked.Increment(ref _reserved) - 1;
		SlimRecord[] records = _records;
		if (i >= records.Length)
		{
			records = GrowToFit(i);
		}

		records[i].Seq = seq;
		records[i].P1 = parameter1;
		records[i].P2 = parameter2;
		Interlocked.Increment(ref _published);
	}

	/// <summary>
	///     Typed count walk used by Verify count terminators (Once/Exactly/Never/AtLeast/...).
	///     Walks the published prefix of <see cref="_records" /> without ever allocating a
	///     <see cref="MethodInvocation{T1, T2}" />.
	/// </summary>
	public int CountMatching(IParameterMatch<T1> match1, IParameterMatch<T2> match2)
	{
		int n = Volatile.Read(ref _published);
		SlimRecord[] records = _records;
		int hit = 0;
		for (int i = 0; i < n; i++)
		{
			ref SlimRecord r = ref records[i];
			if (match1.Matches(r.P1) && match2.Matches(r.P2))
			{
				hit++;
			}
		}

		return hit;
	}

	public void Clear()
	{
		lock (_growLock)
		{
			_reserved = 0;
			Volatile.Write(ref _published, 0);
			_boxed = null;
		}
	}

	void IFastMemberBuffer.AppendBoxed(List<(long Seq, IInteraction Interaction)> dest)
	{
		int n = Volatile.Read(ref _published);
		SlimRecord[] records = _records;
		Dictionary<int, MethodInvocation<T1, T2>> boxed;
		lock (_growLock)
		{
			boxed = _boxed ??= [];
		}

		for (int i = 0; i < n; i++)
		{
			ref SlimRecord r = ref records[i];
			if (!boxed.TryGetValue(i, out MethodInvocation<T1, T2>? invocation))
			{
				invocation = new MethodInvocation<T1, T2>(_methodName, r.P1, r.P2);
				lock (_growLock)
				{
					boxed[i] = invocation;
				}
			}

			dest.Add((r.Seq, invocation));
		}
	}

	private SlimRecord[] GrowToFit(int index)
	{
		lock (_growLock)
		{
			while (index >= _records.Length)
			{
				SlimRecord[] bigger = new SlimRecord[_records.Length * 2];
				Array.Copy(_records, bigger, _records.Length);
				_records = bigger;
			}

			return _records;
		}
	}

	private struct SlimRecord
	{
		public long Seq;
		public T1 P1;
		public T2 P2;
	}
}

/// <summary>
///     Mirror of <see cref="FastMethodBufferFactory" />.<c>InstallMethod</c> that installs the
///     optimized buffer at the given member id.
/// </summary>
public static class FastMethod2BufferOptimizedFactory
{
	public static FastMethod2BufferOptimized<T1, T2> InstallMethodOptimized<T1, T2>(
		this FastMockInteractions interactions, int memberId, string methodName)
	{
		FastMethod2BufferOptimized<T1, T2> buffer = new(interactions, methodName);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}
}
