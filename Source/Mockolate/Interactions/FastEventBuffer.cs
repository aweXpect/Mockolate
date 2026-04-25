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
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastEventBuffer(FastMockInteractions owner, FastEventBufferKind kind)
	{
		_owner = owner;
		_kind = kind;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records an event subscription or unsubscription.
	/// </summary>
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

			_records[n].Seq = seq;
			_records[n].Name = name;
			_records[n].Target = target;
			_records[n].Method = method;
			_records[n].Boxed = null;
			Volatile.Write(ref _count, n + 1);
		}

		_owner.RaiseAdded();
	}

	/// <inheritdoc cref="IFastMemberBuffer.Clear" />
	public void Clear()
	{
		lock (_lock)
		{
			_count = 0;
		}
	}

	void IFastMemberBuffer.AppendBoxed(List<(long Seq, IInteraction Interaction)> dest)
	{
		lock (_lock)
		{
			int n = _count;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref _records[i];
				r.Boxed ??= _kind == FastEventBufferKind.Subscribe
					? new EventSubscription(r.Name, r.Target, r.Method)
					: (IInteraction)new EventUnsubscription(r.Name, r.Target, r.Method);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

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
