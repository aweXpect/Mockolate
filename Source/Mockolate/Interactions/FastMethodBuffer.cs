using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Mockolate.Parameters;

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
	private readonly MockolateLock _growLock = new();
	private Record[] _records;
	private bool[] _verifiedSlots;
	private int _reserved;
	private int _published;

	internal FastMethod0Buffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
		_verifiedSlots = new bool[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records a parameterless method call.
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
			if (_verifiedSlots.Length < records.Length)
			{
				bool[] biggerBits = new bool[records.Length];
				Array.Copy(_verifiedSlots, biggerBits, _verifiedSlots.Length);
				_verifiedSlots = biggerBits;
			}

			return records;
		}
	}

	/// <inheritdoc cref="IFastMemberBuffer.Clear" />
	public void Clear()
	{
		lock (_growLock)
		{
			Array.Clear(_records, 0, _published);
			_reserved = 0;
			Volatile.Write(ref _published, 0);
			Array.Clear(_verifiedSlots, 0, _verifiedSlots.Length);
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
				r.Boxed ??= new MethodInvocation(r.Name);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	void IFastMemberBuffer.AppendBoxedUnverified(List<(long Seq, IInteraction Interaction)> dest)
	{
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			bool[] verified = _verifiedSlots;
			for (int i = 0; i < n; i++)
			{
				if (verified[i])
				{
					continue;
				}

				ref Record r = ref records[i];
				r.Boxed ??= new MethodInvocation(r.Name);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Returns the number of recorded calls and marks every currently-published slot as verified
	///     so a subsequent <see cref="MockInteractions.GetUnverifiedInteractions" /> walk skips them.
	///     Parameterless overload of the typed
	///     <see cref="FastMethod1Buffer{T1}.ConsumeMatching(IParameterMatch{T1})" /> family — provided so
	///     count-only verification can dispatch uniformly across arities without allocating a boxed
	///     <see cref="IInteraction" /> per recorded call. The name reflects the side effect: this is a
	///     <c>Count</c> + <c>MarkVerified</c> step, not a pure read.
	/// </summary>
	public int ConsumeMatching()
	{
		lock (_growLock)
		{
			int n = _published;
			bool[] verified = _verifiedSlots;
			for (int i = 0; i < n; i++)
			{
				verified[i] = true;
			}

			return n;
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
	private readonly MockolateLock _growLock = new();
	private Record[] _records;
	private bool[] _verifiedSlots;
	private int _reserved;
	private int _published;

	internal FastMethod1Buffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
		_verifiedSlots = new bool[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records a 1-parameter method call.
	/// </summary>
	public void Append(string name, T1 parameter1)
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
		records[slot].P1 = parameter1;
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
			if (_verifiedSlots.Length < records.Length)
			{
				bool[] biggerBits = new bool[records.Length];
				Array.Copy(_verifiedSlots, biggerBits, _verifiedSlots.Length);
				_verifiedSlots = biggerBits;
			}

			return records;
		}
	}

	/// <inheritdoc cref="IFastMemberBuffer.Clear" />
	public void Clear()
	{
		lock (_growLock)
		{
			Array.Clear(_records, 0, _published);
			_reserved = 0;
			Volatile.Write(ref _published, 0);
			Array.Clear(_verifiedSlots, 0, _verifiedSlots.Length);
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
				r.Boxed ??= new MethodInvocation<T1>(r.Name, r.P1);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	void IFastMemberBuffer.AppendBoxedUnverified(List<(long Seq, IInteraction Interaction)> dest)
	{
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			bool[] verified = _verifiedSlots;
			for (int i = 0; i < n; i++)
			{
				if (verified[i])
				{
					continue;
				}

				ref Record r = ref records[i];
				r.Boxed ??= new MethodInvocation<T1>(r.Name, r.P1);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded calls whose parameter satisfies <paramref name="match1" />, marking each
	///     matched slot as verified so it is skipped by a later
	///     <see cref="MockInteractions.GetUnverifiedInteractions" /> walk. Walks the typed storage in
	///     place and never allocates an <see cref="IInteraction" />, so count-only verification is
	///     allocation-free in the common case.
	/// </summary>
	public int ConsumeMatching(IParameterMatch<T1> match1)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			bool[] verified = _verifiedSlots;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match1.Matches(r.P1))
				{
					matches++;
					verified[i] = true;
				}
			}
		}

		return matches;
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
	private readonly MockolateLock _growLock = new();
	private Record[] _records;
	private bool[] _verifiedSlots;
	private int _reserved;
	private int _published;

	internal FastMethod2Buffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
		_verifiedSlots = new bool[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records a 2-parameter method call.
	/// </summary>
	public void Append(string name, T1 parameter1, T2 parameter2)
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
		records[slot].P1 = parameter1;
		records[slot].P2 = parameter2;
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
			if (_verifiedSlots.Length < records.Length)
			{
				bool[] biggerBits = new bool[records.Length];
				Array.Copy(_verifiedSlots, biggerBits, _verifiedSlots.Length);
				_verifiedSlots = biggerBits;
			}

			return records;
		}
	}

	/// <inheritdoc cref="IFastMemberBuffer.Clear" />
	public void Clear()
	{
		lock (_growLock)
		{
			Array.Clear(_records, 0, _published);
			_reserved = 0;
			Volatile.Write(ref _published, 0);
			Array.Clear(_verifiedSlots, 0, _verifiedSlots.Length);
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
				r.Boxed ??= new MethodInvocation<T1, T2>(r.Name, r.P1, r.P2);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	void IFastMemberBuffer.AppendBoxedUnverified(List<(long Seq, IInteraction Interaction)> dest)
	{
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			bool[] verified = _verifiedSlots;
			for (int i = 0; i < n; i++)
			{
				if (verified[i])
				{
					continue;
				}

				ref Record r = ref records[i];
				r.Boxed ??= new MethodInvocation<T1, T2>(r.Name, r.P1, r.P2);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded calls whose parameters satisfy the supplied matchers, marking each matched
	///     slot as verified. Allocation-free fast path for count-only verification; mirrors
	///     <see cref="FastMethod1Buffer{T1}.ConsumeMatching(IParameterMatch{T1})" />.
	/// </summary>
	public int ConsumeMatching(IParameterMatch<T1> match1, IParameterMatch<T2> match2)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			bool[] verified = _verifiedSlots;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match1.Matches(r.P1) && match2.Matches(r.P2))
				{
					matches++;
					verified[i] = true;
				}
			}
		}

		return matches;
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
	private readonly MockolateLock _growLock = new();
	private Record[] _records;
	private bool[] _verifiedSlots;
	private int _reserved;
	private int _published;

	internal FastMethod3Buffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
		_verifiedSlots = new bool[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records a 3-parameter method call.
	/// </summary>
	public void Append(string name, T1 parameter1, T2 parameter2, T3 parameter3)
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
			if (_verifiedSlots.Length < records.Length)
			{
				bool[] biggerBits = new bool[records.Length];
				Array.Copy(_verifiedSlots, biggerBits, _verifiedSlots.Length);
				_verifiedSlots = biggerBits;
			}

			return records;
		}
	}

	/// <inheritdoc cref="IFastMemberBuffer.Clear" />
	public void Clear()
	{
		lock (_growLock)
		{
			Array.Clear(_records, 0, _published);
			_reserved = 0;
			Volatile.Write(ref _published, 0);
			Array.Clear(_verifiedSlots, 0, _verifiedSlots.Length);
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
				r.Boxed ??= new MethodInvocation<T1, T2, T3>(r.Name, r.P1, r.P2, r.P3);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	void IFastMemberBuffer.AppendBoxedUnverified(List<(long Seq, IInteraction Interaction)> dest)
	{
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			bool[] verified = _verifiedSlots;
			for (int i = 0; i < n; i++)
			{
				if (verified[i])
				{
					continue;
				}

				ref Record r = ref records[i];
				r.Boxed ??= new MethodInvocation<T1, T2, T3>(r.Name, r.P1, r.P2, r.P3);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded calls whose parameters satisfy the supplied matchers, marking each matched
	///     slot as verified. Allocation-free fast path for count-only verification.
	/// </summary>
	public int ConsumeMatching(IParameterMatch<T1> match1, IParameterMatch<T2> match2, IParameterMatch<T3> match3)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			bool[] verified = _verifiedSlots;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match1.Matches(r.P1) && match2.Matches(r.P2) && match3.Matches(r.P3))
				{
					matches++;
					verified[i] = true;
				}
			}
		}

		return matches;
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
	private readonly MockolateLock _growLock = new();
	private Record[] _records;
	private bool[] _verifiedSlots;
	private int _reserved;
	private int _published;

	internal FastMethod4Buffer(FastMockInteractions owner)
	{
		_owner = owner;
		_records = new Record[4];
		_verifiedSlots = new bool[4];
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => Volatile.Read(ref _published);

	/// <summary>
	///     Records a 4-parameter method call.
	/// </summary>
	public void Append(string name, T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4)
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
			if (_verifiedSlots.Length < records.Length)
			{
				bool[] biggerBits = new bool[records.Length];
				Array.Copy(_verifiedSlots, biggerBits, _verifiedSlots.Length);
				_verifiedSlots = biggerBits;
			}

			return records;
		}
	}

	/// <inheritdoc cref="IFastMemberBuffer.Clear" />
	public void Clear()
	{
		lock (_growLock)
		{
			Array.Clear(_records, 0, _published);
			_reserved = 0;
			Volatile.Write(ref _published, 0);
			Array.Clear(_verifiedSlots, 0, _verifiedSlots.Length);
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
				r.Boxed ??= new MethodInvocation<T1, T2, T3, T4>(r.Name, r.P1, r.P2, r.P3, r.P4);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	void IFastMemberBuffer.AppendBoxedUnverified(List<(long Seq, IInteraction Interaction)> dest)
	{
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			bool[] verified = _verifiedSlots;
			for (int i = 0; i < n; i++)
			{
				if (verified[i])
				{
					continue;
				}

				ref Record r = ref records[i];
				r.Boxed ??= new MethodInvocation<T1, T2, T3, T4>(r.Name, r.P1, r.P2, r.P3, r.P4);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded calls whose parameters satisfy the supplied matchers, marking each matched
	///     slot as verified. Allocation-free fast path for count-only verification.
	/// </summary>
	public int ConsumeMatching(IParameterMatch<T1> match1, IParameterMatch<T2> match2, IParameterMatch<T3> match3, IParameterMatch<T4> match4)
	{
		int matches = 0;
		lock (_growLock)
		{
			int n = _published;
			Record[] records = _records;
			bool[] verified = _verifiedSlots;
			for (int i = 0; i < n; i++)
			{
				ref Record r = ref records[i];
				if (match1.Matches(r.P1) && match2.Matches(r.P2) && match3.Matches(r.P3) && match4.Matches(r.P4))
				{
					matches++;
					verified[i] = true;
				}
			}
		}

		return matches;
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
