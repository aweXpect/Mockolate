using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Mockolate.Interactions;

/// <summary>
///     Distinguishes whether a <see cref="FastEventBuffer" /> records subscribe or unsubscribe calls.
/// </summary>
public enum FastEventBufferKind
{
	/// <summary>The buffer records event subscriptions.</summary>
	Subscribe,
	/// <summary>The buffer records event unsubscriptions.</summary>
	Unsubscribe,
}

/// <summary>
///     Per-member buffer for event subscribe or unsubscribe access. The kind is fixed at construction
///     time so the boxed records produce <see cref="EventSubscription" /> or
///     <see cref="EventUnsubscription" /> as appropriate.
/// </summary>
[DebuggerDisplay("{Count} event {_kind}s")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastEventBuffer : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
	private readonly FastEventBufferKind _kind;
#if NET10_0_OR_GREATER
	private readonly Lock _growLock = new();
#else
	private readonly object _growLock = new();
#endif
	private Record[] _records;
	private int _reserved;
	private int _published;

	internal FastEventBuffer(FastMockInteractions owner, FastEventBufferKind kind)
	{
		_owner = owner;
		_kind = kind;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records an event subscription or unsubscription.
	/// </summary>
	public void Append(string name, object? target, MethodInfo method)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].Name = name;
		records[slot].Target = target;
		records[slot].Method = method;
		records[slot].Boxed = null;
		Interlocked.Increment(ref _published);

		if (_owner.HasInteractionAddedSubscribers)
		{
			_owner.RaiseAdded();
		}
	}

	private Record[] GrowToFit(int slot)
	{
		lock (_growLock)
		{
			Record[] records = _records;
			while (slot >= records.Length)
			{
				Record[] bigger = new Record[records.Length * 2];
				Array.Copy(records, bigger, records.Length);
				records = bigger;
			}

			Volatile.Write(ref _records, records);
			return records;
		}
	}

	/// <inheritdoc cref="IFastMemberBuffer.Clear" />
	public void Clear()
	{
		lock (_growLock)
		{
			_reserved = 0;
			Volatile.Write(ref _published, 0);
		}
	}

	void IFastMemberBuffer.AppendBoxed(List<(long Seq, IInteraction Interaction)> dest)
	{
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				r.Boxed ??= _kind == FastEventBufferKind.Subscribe
					? new EventSubscription(r.Name, r.Target, r.Method)
					: (IInteraction)new EventUnsubscription(r.Name, r.Target, r.Method);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Returns the number of recorded subscribe/unsubscribe accesses. Allocation-free fast path
	///     equivalent to <see cref="Count" />.
	/// </summary>
	public int CountMatching() => Volatile.Read(ref _published);

	private struct Record
	{
		public long Seq;
		public string Name;
		public object? Target;
		public MethodInfo Method;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Factory helpers for event buffers.
/// </summary>
public static class FastEventBufferFactory
{
	/// <summary>
	///     Creates and installs an event subscribe buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastEventBuffer InstallEventSubscribe(this FastMockInteractions interactions, int memberId)
	{
		FastEventBuffer buffer = new(interactions, FastEventBufferKind.Subscribe);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs an event unsubscribe buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastEventBuffer InstallEventUnsubscribe(this FastMockInteractions interactions, int memberId)
	{
		FastEventBuffer buffer = new(interactions, FastEventBufferKind.Unsubscribe);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}
}
