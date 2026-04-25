using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Mockolate.Interactions;

/// <summary>
///     Per-mock interaction storage backed by an array of typed per-member buffers. Each mocked
///     member writes to its own buffer (no shared list, no shared lock); a global monotonic
///     sequence number tags every record so ordered enumeration across buffers preserves
///     registration order.
/// </summary>
[DebuggerDisplay("{Count} interactions")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class FastMockInteractions : IMockInteractions
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly IFastMemberBuffer?[] _buffers;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private long _globalSequence;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#if NET10_0_OR_GREATER
	private readonly Lock _verifiedLock = new();
#else
	private readonly object _verifiedLock = new();
#endif

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private HashSet<IInteraction>? _verified;

	/// <summary>
	///     Creates a new <see cref="FastMockInteractions" /> sized to <paramref name="memberCount" />.
	///     Each mockable member is later assigned its own per-member buffer at the matching index via
	///     <see cref="InstallBuffer(int, IFastMemberBuffer)" />.
	/// </summary>
	/// <param name="memberCount">The number of distinct mockable members the buffer array should hold.</param>
	/// <param name="skipInteractionRecording">Mirrors <see cref="MockBehavior.SkipInteractionRecording" />.</param>
	public FastMockInteractions(int memberCount, bool skipInteractionRecording = false)
	{
		_buffers = new IFastMemberBuffer?[memberCount];
		SkipInteractionRecording = skipInteractionRecording;
	}

	/// <summary>
	///     The per-member buffers exposed by index. The source generator emits typed casts against
	///     this array to record interactions without going through the legacy
	///     <see cref="IMockInteractions.RegisterInteraction{TInteraction}(TInteraction)" /> path.
	/// </summary>
	public IFastMemberBuffer?[] Buffers => _buffers;

	/// <inheritdoc cref="IMockInteractions.SkipInteractionRecording" />
	public bool SkipInteractionRecording { get; }

	/// <inheritdoc cref="IMockInteractions.InteractionAdded" />
	public event EventHandler? InteractionAdded;

	/// <inheritdoc cref="IMockInteractions.OnClearing" />
	public event EventHandler? OnClearing;

	/// <summary>
	///     The number of interactions contained in the collection across all per-member buffers.
	/// </summary>
	public int Count => (int)Interlocked.Read(ref _globalSequence);

	/// <summary>
	///     Installs <paramref name="buffer" /> at the slot matching <paramref name="memberId" />.
	///     Called once at mock construction by the source generator.
	/// </summary>
	/// <param name="memberId">The generator-emitted member id for the buffer's target member.</param>
	/// <param name="buffer">The per-member buffer to install.</param>
	public void InstallBuffer(int memberId, IFastMemberBuffer buffer)
		=> _buffers[memberId] = buffer;

	/// <summary>
	///     Reserves the next sequence number for a recording. Buffer implementations call this once
	///     per <see cref="IFastMemberBuffer" /> append so cross-buffer enumeration preserves
	///     registration order.
	/// </summary>
	/// <remarks>
	///     Public because the source generator emits arity-N buffers in the consumer assembly that
	///     need to participate in the same sequence stream. Not intended for end-user code.
	/// </remarks>
	public long NextSequence()
		=> Interlocked.Increment(ref _globalSequence) - 1;

	/// <summary>
	///     Fires the <see cref="InteractionAdded" /> event. Buffer implementations call this after
	///     publishing a new record.
	/// </summary>
	/// <remarks>
	///     Public because the source generator emits arity-N buffers in the consumer assembly that
	///     need to drive the event. Not intended for end-user code.
	/// </remarks>
	public void RaiseAdded()
		=> InteractionAdded?.Invoke(this, EventArgs.Empty);

	/// <inheritdoc cref="IMockInteractions.RegisterInteraction{TInteraction}(TInteraction)" />
	/// <remarks>
	///     Transitional fallback for code paths that still produce a heap-allocated
	///     <see cref="IInteraction" /> instance. The hot-path generator emission bypasses this entirely
	///     and writes straight into the typed per-member buffer. <c>RegisterInteraction</c> here
	///     dispatches by interaction type to the matching buffer when the receiver knows its member id;
	///     otherwise the record falls back to a global "unattributed" buffer that still participates
	///     in ordered enumeration and verification.
	/// </remarks>
	TInteraction IMockInteractions.RegisterInteraction<TInteraction>(TInteraction interaction)
	{
		if (SkipInteractionRecording)
		{
			return interaction;
		}

		IFastMemberBuffer fallback = GetOrCreateFallbackBuffer();
		((FallbackBuffer)fallback).Append(interaction);
		return interaction;
	}

	/// <inheritdoc cref="IMockInteractions.GetUnverifiedInteractions" />
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
			foreach (IInteraction interaction in snapshot)
			{
				if (!_verified.Contains(interaction))
				{
					result.Add(interaction);
				}
			}

			return result;
		}
	}

	void IMockInteractions.Verified(IEnumerable<IInteraction> interactions)
	{
		lock (_verifiedLock)
		{
			_verified ??= [];
			foreach (IInteraction interaction in interactions)
			{
				_verified.Add(interaction);
			}
		}
	}

	/// <inheritdoc cref="IMockInteractions.Clear" />
	public void Clear()
	{
		Interlocked.Exchange(ref _globalSequence, 0);
		foreach (IFastMemberBuffer? buffer in _buffers)
		{
			buffer?.Clear();
		}

		Volatile.Read(ref _fallback)?.Clear();

		lock (_verifiedLock)
		{
			_verified = null;
		}

		OnClearing?.Invoke(this, EventArgs.Empty);
	}

	/// <inheritdoc cref="IEnumerable{IInteraction}.GetEnumerator()" />
	public IEnumerator<IInteraction> GetEnumerator()
		=> ((IEnumerable<IInteraction>)SnapshotOrdered()).GetEnumerator();

	/// <inheritdoc cref="IEnumerable.GetEnumerator()" />
	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();

	private IInteraction[] SnapshotOrdered()
	{
		List<(long Seq, IInteraction Interaction)> all = new();
		foreach (IFastMemberBuffer? buffer in _buffers)
		{
			buffer?.AppendBoxed(all);
		}

		Volatile.Read(ref _fallback)?.AppendBoxed(all);

		all.Sort(static (left, right) => left.Seq.CompareTo(right.Seq));
		IInteraction[] result = new IInteraction[all.Count];
		for (int i = 0; i < all.Count; i++)
		{
			result[i] = all[i].Interaction;
		}

		return result;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private FallbackBuffer? _fallback;

	private IFastMemberBuffer GetOrCreateFallbackBuffer()
	{
		FallbackBuffer? buffer = Volatile.Read(ref _fallback);
		if (buffer is not null)
		{
			return buffer;
		}

		FallbackBuffer created = new(this);
		FallbackBuffer? existing = Interlocked.CompareExchange(ref _fallback, created, null);
		return existing ?? created;
	}

	private sealed class FallbackBuffer(FastMockInteractions owner) : IFastMemberBuffer
	{
		private readonly FastMockInteractions _owner = owner;
#if NET10_0_OR_GREATER
		private readonly Lock _lock = new();
#else
		private readonly object _lock = new();
#endif
		private (long Seq, IInteraction Interaction)[] _records = new (long, IInteraction)[4];
		private int _count;

		public int Count => Volatile.Read(ref _count);

		public void Append(IInteraction interaction)
		{
			long seq = _owner.NextSequence();
			lock (_lock)
			{
				int n = _count;
				if (n == _records.Length)
				{
					Array.Resize(ref _records, n * 2);
				}

				_records[n] = (seq, interaction);
				Volatile.Write(ref _count, n + 1);
			}

			_owner.RaiseAdded();
		}

		public void Clear()
		{
			lock (_lock)
			{
				_count = 0;
			}
		}

		public void AppendBoxed(List<(long Seq, IInteraction Interaction)> dest)
		{
			(long Seq, IInteraction Interaction)[] snapshot;
			int n;
			lock (_lock)
			{
				n = _count;
				snapshot = _records;
			}

			for (int i = 0; i < n; i++)
			{
				dest.Add(snapshot[i]);
			}
		}
	}
}
