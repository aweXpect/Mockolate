using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Mockolate.Interactions;

/// <summary>
///     Per-member buffer for indexer getters with a single key.
/// </summary>
[DebuggerDisplay("{Count} indexer gets")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastIndexerGetterBuffer<T1> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastIndexerGetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records an indexer getter access.
	/// </summary>
	public void Append(T1 parameter1)
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
			_records[n].P1 = parameter1;
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
				r.Boxed ??= new IndexerGetterAccess<T1>(r.P1);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	private struct Record
	{
		public long Seq;
		public T1 P1;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Per-member buffer for indexer getters with two keys.
/// </summary>
[DebuggerDisplay("{Count} indexer gets")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastIndexerGetterBuffer<T1, T2> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastIndexerGetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records an indexer getter access.
	/// </summary>
	public void Append(T1 parameter1, T2 parameter2)
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
			_records[n].P1 = parameter1;
			_records[n].P2 = parameter2;
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
				r.Boxed ??= new IndexerGetterAccess<T1, T2>(r.P1, r.P2);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	private struct Record
	{
		public long Seq;
		public T1 P1;
		public T2 P2;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Per-member buffer for indexer getters with three keys.
/// </summary>
[DebuggerDisplay("{Count} indexer gets")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastIndexerGetterBuffer<T1, T2, T3> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastIndexerGetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records an indexer getter access.
	/// </summary>
	public void Append(T1 parameter1, T2 parameter2, T3 parameter3)
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
			_records[n].P1 = parameter1;
			_records[n].P2 = parameter2;
			_records[n].P3 = parameter3;
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
				r.Boxed ??= new IndexerGetterAccess<T1, T2, T3>(r.P1, r.P2, r.P3);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	private struct Record
	{
		public long Seq;
		public T1 P1;
		public T2 P2;
		public T3 P3;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Per-member buffer for indexer getters with four keys.
/// </summary>
[DebuggerDisplay("{Count} indexer gets")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastIndexerGetterBuffer<T1, T2, T3, T4> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastIndexerGetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records an indexer getter access.
	/// </summary>
	public void Append(T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4)
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
			_records[n].P1 = parameter1;
			_records[n].P2 = parameter2;
			_records[n].P3 = parameter3;
			_records[n].P4 = parameter4;
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
				r.Boxed ??= new IndexerGetterAccess<T1, T2, T3, T4>(r.P1, r.P2, r.P3, r.P4);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	private struct Record
	{
		public long Seq;
		public T1 P1;
		public T2 P2;
		public T3 P3;
		public T4 P4;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Per-member buffer for indexer setters with a single key.
/// </summary>
[DebuggerDisplay("{Count} indexer sets")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastIndexerSetterBuffer<T1, TValue> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastIndexerSetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records an indexer setter access.
	/// </summary>
	public void Append(T1 parameter1, TValue value)
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
			_records[n].P1 = parameter1;
			_records[n].Value = value;
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
				r.Boxed ??= new IndexerSetterAccess<T1, TValue>(r.P1, r.Value);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	private struct Record
	{
		public long Seq;
		public T1 P1;
		public TValue Value;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Per-member buffer for indexer setters with two keys.
/// </summary>
[DebuggerDisplay("{Count} indexer sets")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastIndexerSetterBuffer<T1, T2, TValue> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastIndexerSetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records an indexer setter access.
	/// </summary>
	public void Append(T1 parameter1, T2 parameter2, TValue value)
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
			_records[n].P1 = parameter1;
			_records[n].P2 = parameter2;
			_records[n].Value = value;
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
				r.Boxed ??= new IndexerSetterAccess<T1, T2, TValue>(r.P1, r.P2, r.Value);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	private struct Record
	{
		public long Seq;
		public T1 P1;
		public T2 P2;
		public TValue Value;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Per-member buffer for indexer setters with three keys.
/// </summary>
[DebuggerDisplay("{Count} indexer sets")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastIndexerSetterBuffer<T1, T2, T3, TValue> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastIndexerSetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records an indexer setter access.
	/// </summary>
	public void Append(T1 parameter1, T2 parameter2, T3 parameter3, TValue value)
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
			_records[n].P1 = parameter1;
			_records[n].P2 = parameter2;
			_records[n].P3 = parameter3;
			_records[n].Value = value;
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
				r.Boxed ??= new IndexerSetterAccess<T1, T2, T3, TValue>(r.P1, r.P2, r.P3, r.Value);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	private struct Record
	{
		public long Seq;
		public T1 P1;
		public T2 P2;
		public T3 P3;
		public TValue Value;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Per-member buffer for indexer setters with four keys.
/// </summary>
[DebuggerDisplay("{Count} indexer sets")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastIndexerSetterBuffer<T1, T2, T3, T4, TValue> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastIndexerSetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records an indexer setter access.
	/// </summary>
	public void Append(T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4, TValue value)
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
			_records[n].P1 = parameter1;
			_records[n].P2 = parameter2;
			_records[n].P3 = parameter3;
			_records[n].P4 = parameter4;
			_records[n].Value = value;
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
				r.Boxed ??= new IndexerSetterAccess<T1, T2, T3, T4, TValue>(r.P1, r.P2, r.P3, r.P4, r.Value);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	private struct Record
	{
		public long Seq;
		public T1 P1;
		public T2 P2;
		public T3 P3;
		public T4 P4;
		public TValue Value;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Factory helpers for indexer buffers.
/// </summary>
public static class FastIndexerBufferFactory
{
	/// <summary>
	///     Creates and installs an indexer getter buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastIndexerGetterBuffer<T1> InstallIndexerGetter<T1>(this FastMockInteractions interactions, int memberId)
	{
		FastIndexerGetterBuffer<T1> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs an indexer getter buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastIndexerGetterBuffer<T1, T2> InstallIndexerGetter<T1, T2>(this FastMockInteractions interactions, int memberId)
	{
		FastIndexerGetterBuffer<T1, T2> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs an indexer getter buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastIndexerGetterBuffer<T1, T2, T3> InstallIndexerGetter<T1, T2, T3>(this FastMockInteractions interactions, int memberId)
	{
		FastIndexerGetterBuffer<T1, T2, T3> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs an indexer getter buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastIndexerGetterBuffer<T1, T2, T3, T4> InstallIndexerGetter<T1, T2, T3, T4>(this FastMockInteractions interactions, int memberId)
	{
		FastIndexerGetterBuffer<T1, T2, T3, T4> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs an indexer setter buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastIndexerSetterBuffer<T1, TValue> InstallIndexerSetter<T1, TValue>(this FastMockInteractions interactions, int memberId)
	{
		FastIndexerSetterBuffer<T1, TValue> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs an indexer setter buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastIndexerSetterBuffer<T1, T2, TValue> InstallIndexerSetter<T1, T2, TValue>(this FastMockInteractions interactions, int memberId)
	{
		FastIndexerSetterBuffer<T1, T2, TValue> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs an indexer setter buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastIndexerSetterBuffer<T1, T2, T3, TValue> InstallIndexerSetter<T1, T2, T3, TValue>(this FastMockInteractions interactions, int memberId)
	{
		FastIndexerSetterBuffer<T1, T2, T3, TValue> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs an indexer setter buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastIndexerSetterBuffer<T1, T2, T3, T4, TValue> InstallIndexerSetter<T1, T2, T3, T4, TValue>(this FastMockInteractions interactions, int memberId)
	{
		FastIndexerSetterBuffer<T1, T2, T3, T4, TValue> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}
}
