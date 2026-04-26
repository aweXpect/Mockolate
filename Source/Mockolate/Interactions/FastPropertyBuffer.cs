using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Mockolate.Parameters;

namespace Mockolate.Interactions;

/// <summary>
///     Per-member buffer for property getters. Records only the property name + sequence number.
/// </summary>
[DebuggerDisplay("{Count} property gets")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastPropertyGetterBuffer : IFastMemberBuffer
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

	internal FastPropertyGetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records a property getter access.
	/// </summary>
	public void Append(string name)
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
				r.Boxed ??= new PropertyGetterAccess(r.Name);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Returns the number of recorded property getter accesses. Allocation-free fast path
	///     equivalent to <see cref="Count" />, exposed for symmetry with the matcher-taking
	///     <see cref="FastPropertySetterBuffer{T}.CountMatching(IParameterMatch{T})" />.
	/// </summary>
	public int CountMatching() => Volatile.Read(ref _published);

	private struct Record
	{
		public long Seq;
		public string Name;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Per-member buffer for property setters. Records the property name and the assigned value.
/// </summary>
[DebuggerDisplay("{Count} property sets")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastPropertySetterBuffer<T> : IFastMemberBuffer
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

	internal FastPropertySetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records a property setter access.
	/// </summary>
	public void Append(string name, T value)
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
		records[slot].Value = value;
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
				r.Boxed ??= new PropertySetterAccess<T>(r.Name, r.Value);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded setter accesses whose assigned value satisfies <paramref name="match" />.
	///     Allocation-free fast path for count-only verification.
	/// </summary>
	public int CountMatching(IParameterMatch<T> match)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match.Matches(r.Value))
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
		public string Name;
		public T Value;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Factory helpers for property buffers.
/// </summary>
public static class FastPropertyBufferFactory
{
	/// <summary>
	///     Creates and installs a property getter buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastPropertyGetterBuffer InstallPropertyGetter(this FastMockInteractions interactions, int memberId)
	{
		FastPropertyGetterBuffer buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs a property setter buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastPropertySetterBuffer<T> InstallPropertySetter<T>(this FastMockInteractions interactions, int memberId)
	{
		FastPropertySetterBuffer<T> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}
}
