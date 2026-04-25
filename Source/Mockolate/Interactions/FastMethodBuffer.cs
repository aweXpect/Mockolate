using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Mockolate.Interactions;

/// <summary>
///     Per-member buffer for parameterless methods. Records only the method name + sequence number.
/// </summary>
[DebuggerDisplay("{Count} method calls")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastMethod0Buffer : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastMethod0Buffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records a parameterless method call.
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
				r.Boxed ??= new MethodInvocation(r.Name);
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
///     Per-member buffer for 1-parameter methods.
/// </summary>
[DebuggerDisplay("{Count} method calls")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastMethod1Buffer<T1> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastMethod1Buffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records a 1-parameter method call.
	/// </summary>
	public void Append(string name, T1 parameter1)
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
				r.Boxed ??= new MethodInvocation<T1>(r.Name, r.P1);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	private struct Record
	{
		public long Seq;
		public string Name;
		public T1 P1;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Per-member buffer for 2-parameter methods.
/// </summary>
[DebuggerDisplay("{Count} method calls")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastMethod2Buffer<T1, T2> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastMethod2Buffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records a 2-parameter method call.
	/// </summary>
	public void Append(string name, T1 parameter1, T2 parameter2)
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
				r.Boxed ??= new MethodInvocation<T1, T2>(r.Name, r.P1, r.P2);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	private struct Record
	{
		public long Seq;
		public string Name;
		public T1 P1;
		public T2 P2;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Per-member buffer for 3-parameter methods.
/// </summary>
[DebuggerDisplay("{Count} method calls")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastMethod3Buffer<T1, T2, T3> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastMethod3Buffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records a 3-parameter method call.
	/// </summary>
	public void Append(string name, T1 parameter1, T2 parameter2, T3 parameter3)
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
				r.Boxed ??= new MethodInvocation<T1, T2, T3>(r.Name, r.P1, r.P2, r.P3);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	private struct Record
	{
		public long Seq;
		public string Name;
		public T1 P1;
		public T2 P2;
		public T3 P3;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Per-member buffer for 4-parameter methods.
/// </summary>
[DebuggerDisplay("{Count} method calls")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastMethod4Buffer<T1, T2, T3, T4> : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private Record[] _records;
	private int _count;

	internal FastMethod4Buffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _count);

	/// <summary>
	///     Records a 4-parameter method call.
	/// </summary>
	public void Append(string name, T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4)
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
				r.Boxed ??= new MethodInvocation<T1, T2, T3, T4>(r.Name, r.P1, r.P2, r.P3, r.P4);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	private struct Record
	{
		public long Seq;
		public string Name;
		public T1 P1;
		public T2 P2;
		public T3 P3;
		public T4 P4;
		public IInteraction? Boxed;
	}
}

/// <summary>
///     Factory helpers that hide the per-arity buffer constructors behind the
///     <see cref="FastMockInteractions" /> API.
/// </summary>
public static class FastMethodBufferFactory
{
	/// <summary>
	///     Creates and installs a parameterless method buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastMethod0Buffer InstallMethod(this FastMockInteractions interactions, int memberId)
	{
		FastMethod0Buffer buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs a 1-parameter method buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastMethod1Buffer<T1> InstallMethod<T1>(this FastMockInteractions interactions, int memberId)
	{
		FastMethod1Buffer<T1> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs a 2-parameter method buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastMethod2Buffer<T1, T2> InstallMethod<T1, T2>(this FastMockInteractions interactions, int memberId)
	{
		FastMethod2Buffer<T1, T2> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs a 3-parameter method buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastMethod3Buffer<T1, T2, T3> InstallMethod<T1, T2, T3>(this FastMockInteractions interactions, int memberId)
	{
		FastMethod3Buffer<T1, T2, T3> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}

	/// <summary>
	///     Creates and installs a 4-parameter method buffer at the given <paramref name="memberId" />.
	/// </summary>
	public static FastMethod4Buffer<T1, T2, T3, T4> InstallMethod<T1, T2, T3, T4>(this FastMockInteractions interactions, int memberId)
	{
		FastMethod4Buffer<T1, T2, T3, T4> buffer = new(interactions);
		interactions.InstallBuffer(memberId, buffer);
		return buffer;
	}
}
