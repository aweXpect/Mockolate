using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Mockolate.Parameters;

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
	private readonly Lock _growLock = new();
#else
	private readonly object _growLock = new();
#endif
	private Record[] _records;
	private int _reserved;
	private int _published;

	internal FastIndexerGetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records an indexer getter access.
	/// </summary>
	public void Append(T1 parameter1)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = parameter1;
		records[slot].Boxed = null;
		Interlocked.Increment(ref _published);

		if (_owner.HasInteractionAddedSubscribers)
		{
			_owner.RaiseAdded();
		}
	}

	/// <summary>
	///     Records an indexer getter access using an already-allocated <see cref="IndexerGetterAccess{T1}" />.
	/// </summary>
	/// <remarks>
	///     The generator allocates the access value to use as a setup-matching key; this overload reuses
	///     it as the pre-boxed record so enumeration doesn't allocate a second one.
	/// </remarks>
	public void Append(IndexerGetterAccess<T1> access)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = access.Parameter1;
		records[slot].Boxed = access;
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
				r.Boxed ??= new IndexerGetterAccess<T1>(r.P1);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded indexer getter accesses whose key satisfies <paramref name="match1" />.
	///     Allocation-free fast path for count-only verification.
	/// </summary>
	public int CountMatching(IParameterMatch<T1> match1)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match1.Matches(r.P1))
				{
					matches++;
				}
			}
		}

		return matches;
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
	private readonly Lock _growLock = new();
#else
	private readonly object _growLock = new();
#endif
	private Record[] _records;
	private int _reserved;
	private int _published;

	internal FastIndexerGetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records an indexer getter access.
	/// </summary>
	public void Append(T1 parameter1, T2 parameter2)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = parameter1;
		records[slot].P2 = parameter2;
		records[slot].Boxed = null;
		Interlocked.Increment(ref _published);

		if (_owner.HasInteractionAddedSubscribers)
		{
			_owner.RaiseAdded();
		}
	}

	/// <summary>
	///     Records an indexer getter access using an already-allocated <see cref="IndexerGetterAccess{T1, T2}" />.
	/// </summary>
	public void Append(IndexerGetterAccess<T1, T2> access)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = access.Parameter1;
		records[slot].P2 = access.Parameter2;
		records[slot].Boxed = access;
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
				r.Boxed ??= new IndexerGetterAccess<T1, T2>(r.P1, r.P2);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded indexer getter accesses whose keys satisfy the supplied matchers.
	///     Allocation-free fast path for count-only verification.
	/// </summary>
	public int CountMatching(IParameterMatch<T1> match1, IParameterMatch<T2> match2)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match1.Matches(r.P1) && match2.Matches(r.P2))
				{
					matches++;
				}
			}
		}

		return matches;
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
	private readonly Lock _growLock = new();
#else
	private readonly object _growLock = new();
#endif
	private Record[] _records;
	private int _reserved;
	private int _published;

	internal FastIndexerGetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records an indexer getter access.
	/// </summary>
	public void Append(T1 parameter1, T2 parameter2, T3 parameter3)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = parameter1;
		records[slot].P2 = parameter2;
		records[slot].P3 = parameter3;
		records[slot].Boxed = null;
		Interlocked.Increment(ref _published);

		if (_owner.HasInteractionAddedSubscribers)
		{
			_owner.RaiseAdded();
		}
	}

	/// <summary>
	///     Records an indexer getter access using an already-allocated <see cref="IndexerGetterAccess{T1, T2, T3}" />.
	/// </summary>
	public void Append(IndexerGetterAccess<T1, T2, T3> access)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = access.Parameter1;
		records[slot].P2 = access.Parameter2;
		records[slot].P3 = access.Parameter3;
		records[slot].Boxed = access;
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
				r.Boxed ??= new IndexerGetterAccess<T1, T2, T3>(r.P1, r.P2, r.P3);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded indexer getter accesses whose keys satisfy the supplied matchers.
	///     Allocation-free fast path for count-only verification.
	/// </summary>
	public int CountMatching(IParameterMatch<T1> match1, IParameterMatch<T2> match2, IParameterMatch<T3> match3)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match1.Matches(r.P1) && match2.Matches(r.P2) && match3.Matches(r.P3))
				{
					matches++;
				}
			}
		}

		return matches;
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
	private readonly Lock _growLock = new();
#else
	private readonly object _growLock = new();
#endif
	private Record[] _records;
	private int _reserved;
	private int _published;

	internal FastIndexerGetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records an indexer getter access.
	/// </summary>
	public void Append(T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = parameter1;
		records[slot].P2 = parameter2;
		records[slot].P3 = parameter3;
		records[slot].P4 = parameter4;
		records[slot].Boxed = null;
		Interlocked.Increment(ref _published);

		if (_owner.HasInteractionAddedSubscribers)
		{
			_owner.RaiseAdded();
		}
	}

	/// <summary>
	///     Records an indexer getter access using an already-allocated <see cref="IndexerGetterAccess{T1, T2, T3, T4}" />.
	/// </summary>
	public void Append(IndexerGetterAccess<T1, T2, T3, T4> access)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = access.Parameter1;
		records[slot].P2 = access.Parameter2;
		records[slot].P3 = access.Parameter3;
		records[slot].P4 = access.Parameter4;
		records[slot].Boxed = access;
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
				r.Boxed ??= new IndexerGetterAccess<T1, T2, T3, T4>(r.P1, r.P2, r.P3, r.P4);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded indexer getter accesses whose keys satisfy the supplied matchers.
	///     Allocation-free fast path for count-only verification.
	/// </summary>
	public int CountMatching(IParameterMatch<T1> match1, IParameterMatch<T2> match2, IParameterMatch<T3> match3, IParameterMatch<T4> match4)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match1.Matches(r.P1) && match2.Matches(r.P2) && match3.Matches(r.P3) && match4.Matches(r.P4))
				{
					matches++;
				}
			}
		}

		return matches;
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
	private readonly Lock _growLock = new();
#else
	private readonly object _growLock = new();
#endif
	private Record[] _records;
	private int _reserved;
	private int _published;

	internal FastIndexerSetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records an indexer setter access.
	/// </summary>
	public void Append(T1 parameter1, TValue value)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = parameter1;
		records[slot].Value = value;
		records[slot].Boxed = null;
		Interlocked.Increment(ref _published);

		if (_owner.HasInteractionAddedSubscribers)
		{
			_owner.RaiseAdded();
		}
	}

	/// <summary>
	///     Records an indexer setter access using an already-allocated <see cref="IndexerSetterAccess{T1, TValue}" />.
	/// </summary>
	public void Append(IndexerSetterAccess<T1, TValue> access)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = access.Parameter1;
		records[slot].Value = access.TypedValue;
		records[slot].Boxed = access;
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
				r.Boxed ??= new IndexerSetterAccess<T1, TValue>(r.P1, r.Value);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded indexer setter accesses whose key and assigned value satisfy the supplied
	///     matchers. Allocation-free fast path for count-only verification.
	/// </summary>
	public int CountMatching(IParameterMatch<T1> match1, IParameterMatch<TValue> matchValue)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match1.Matches(r.P1) && matchValue.Matches(r.Value))
				{
					matches++;
				}
			}
		}

		return matches;
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
	private readonly Lock _growLock = new();
#else
	private readonly object _growLock = new();
#endif
	private Record[] _records;
	private int _reserved;
	private int _published;

	internal FastIndexerSetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records an indexer setter access.
	/// </summary>
	public void Append(T1 parameter1, T2 parameter2, TValue value)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = parameter1;
		records[slot].P2 = parameter2;
		records[slot].Value = value;
		records[slot].Boxed = null;
		Interlocked.Increment(ref _published);

		if (_owner.HasInteractionAddedSubscribers)
		{
			_owner.RaiseAdded();
		}
	}

	/// <summary>
	///     Records an indexer setter access using an already-allocated <see cref="IndexerSetterAccess{T1, T2, TValue}" />.
	/// </summary>
	public void Append(IndexerSetterAccess<T1, T2, TValue> access)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = access.Parameter1;
		records[slot].P2 = access.Parameter2;
		records[slot].Value = access.TypedValue;
		records[slot].Boxed = access;
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
				r.Boxed ??= new IndexerSetterAccess<T1, T2, TValue>(r.P1, r.P2, r.Value);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded indexer setter accesses whose keys and assigned value satisfy the supplied
	///     matchers. Allocation-free fast path for count-only verification.
	/// </summary>
	public int CountMatching(IParameterMatch<T1> match1, IParameterMatch<T2> match2, IParameterMatch<TValue> matchValue)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match1.Matches(r.P1) && match2.Matches(r.P2) && matchValue.Matches(r.Value))
				{
					matches++;
				}
			}
		}

		return matches;
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
	private readonly Lock _growLock = new();
#else
	private readonly object _growLock = new();
#endif
	private Record[] _records;
	private int _reserved;
	private int _published;

	internal FastIndexerSetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records an indexer setter access.
	/// </summary>
	public void Append(T1 parameter1, T2 parameter2, T3 parameter3, TValue value)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = parameter1;
		records[slot].P2 = parameter2;
		records[slot].P3 = parameter3;
		records[slot].Value = value;
		records[slot].Boxed = null;
		Interlocked.Increment(ref _published);

		if (_owner.HasInteractionAddedSubscribers)
		{
			_owner.RaiseAdded();
		}
	}

	/// <summary>
	///     Records an indexer setter access using an already-allocated <see cref="IndexerSetterAccess{T1, T2, T3, TValue}" />.
	/// </summary>
	public void Append(IndexerSetterAccess<T1, T2, T3, TValue> access)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = access.Parameter1;
		records[slot].P2 = access.Parameter2;
		records[slot].P3 = access.Parameter3;
		records[slot].Value = access.TypedValue;
		records[slot].Boxed = access;
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
				r.Boxed ??= new IndexerSetterAccess<T1, T2, T3, TValue>(r.P1, r.P2, r.P3, r.Value);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded indexer setter accesses whose keys and assigned value satisfy the supplied
	///     matchers. Allocation-free fast path for count-only verification.
	/// </summary>
	public int CountMatching(IParameterMatch<T1> match1, IParameterMatch<T2> match2, IParameterMatch<T3> match3, IParameterMatch<TValue> matchValue)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match1.Matches(r.P1) && match2.Matches(r.P2) && match3.Matches(r.P3) && matchValue.Matches(r.Value))
				{
					matches++;
				}
			}
		}

		return matches;
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
	private readonly Lock _growLock = new();
#else
	private readonly object _growLock = new();
#endif
	private Record[] _records;
	private int _reserved;
	private int _published;

	internal FastIndexerSetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records an indexer setter access.
	/// </summary>
	public void Append(T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4, TValue value)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = parameter1;
		records[slot].P2 = parameter2;
		records[slot].P3 = parameter3;
		records[slot].P4 = parameter4;
		records[slot].Value = value;
		records[slot].Boxed = null;
		Interlocked.Increment(ref _published);

		if (_owner.HasInteractionAddedSubscribers)
		{
			_owner.RaiseAdded();
		}
	}

	/// <summary>
	///     Records an indexer setter access using an already-allocated <see cref="IndexerSetterAccess{T1, T2, T3, T4, TValue}" />.
	/// </summary>
	public void Append(IndexerSetterAccess<T1, T2, T3, T4, TValue> access)
	{
		long seq = _owner.NextSequence();
		int slot = Interlocked.Increment(ref _reserved) - 1;
		Record[] records = Volatile.Read(ref _records);
		if (slot >= records.Length)
		{
			records = GrowToFit(slot);
		}

		records[slot].Seq = seq;
		records[slot].P1 = access.Parameter1;
		records[slot].P2 = access.Parameter2;
		records[slot].P3 = access.Parameter3;
		records[slot].P4 = access.Parameter4;
		records[slot].Value = access.TypedValue;
		records[slot].Boxed = access;
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
				r.Boxed ??= new IndexerSetterAccess<T1, T2, T3, T4, TValue>(r.P1, r.P2, r.P3, r.P4, r.Value);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded indexer setter accesses whose keys and assigned value satisfy the supplied
	///     matchers. Allocation-free fast path for count-only verification.
	/// </summary>
	public int CountMatching(IParameterMatch<T1> match1, IParameterMatch<T2> match2, IParameterMatch<T3> match3, IParameterMatch<T4> match4, IParameterMatch<TValue> matchValue)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match1.Matches(r.P1) && match2.Matches(r.P2) && match3.Matches(r.P3) && match4.Matches(r.P4) && matchValue.Matches(r.Value))
				{
					matches++;
				}
			}
		}

		return matches;
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
