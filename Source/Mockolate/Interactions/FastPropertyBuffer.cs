using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastPropertyGetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records a property getter access.
	/// </summary>
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

			_records[n].Seq = seq;
			_records[n].Name = name;
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
				r.Boxed ??= new PropertyGetterAccess(r.Name);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

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
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastPropertySetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records a property setter access.
	/// </summary>
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

			_records[n].Seq = seq;
			_records[n].Name = name;
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
				r.Boxed ??= new PropertySetterAccess<T>(r.Name, r.Value);
				dest.Add((r.Seq, r.Boxed));
			}
		}
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
