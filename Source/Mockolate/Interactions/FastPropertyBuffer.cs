using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mockolate.Parameters;

namespace Mockolate.Interactions;

/// <summary>
///     Per-member buffer for property getters. The buffer is bound to a single property and
///     keeps the property name on a per-buffer <see cref="PropertyGetterAccess" /> template, so
///     the per-call hot path only stores the sequence number. Verification still emits a fresh
///     <see cref="PropertyGetterAccess" /> per recorded slot — the template is only used as a
///     name source — because reference-keyed bookkeeping (Then ordering, the verified set)
///     requires each recorded interaction to be a distinct object.
/// </summary>
[DebuggerDisplay("{Count} property gets")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public sealed class FastPropertyGetterBuffer : IFastMemberBuffer
{
	private readonly FastMockInteractions _owner;
	private readonly ChunkedSlotStorage<Record> _storage = new();
	private PropertyGetterAccess? _access;

	internal FastPropertyGetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
	}

	internal FastPropertyGetterBuffer(FastMockInteractions owner, PropertyGetterAccess access)
	{
		_owner = owner;
		_access = access;
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => _storage.Count;

	/// <summary>
	///     Records a property getter access using the buffer's pre-seeded
	///     <see cref="PropertyGetterAccess" /> template. Throws when the template has not been
	///     installed — callers must use <see cref="Append(string)" /> in that case.
	/// </summary>
	/// <exception cref="InvalidOperationException">No template was supplied at install time.</exception>
	public void Append()
	{
		if (_access is null)
		{
			throw new InvalidOperationException(
				$"{nameof(Append)}() requires the buffer to be installed with a {nameof(PropertyGetterAccess)} template via {nameof(FastPropertyBufferFactory)}.{nameof(FastPropertyBufferFactory.InstallPropertyGetter)}(memberId, access). Use {nameof(Append)}(string) when no template is available.");
		}

		long seq = _owner.NextSequence();
		int slot = _storage.Reserve();
		ref Record r = ref _storage.SlotForWrite(slot);
		r.Seq = seq;
		r.Boxed = null;
		_storage.Publish();

		if (_owner.HasInteractionAddedSubscribers)
		{
			_owner.RaiseAdded();
		}
	}

	/// <summary>
	///     Records a property getter access. Lazily installs the buffer's
	///     <see cref="PropertyGetterAccess" /> template from <paramref name="name" /> on first
	///     call so legacy callers (generated code that does not pass a pre-built template) keep
	///     working without allocating one template object per record.
	/// </summary>
	public void Append(string name)
	{
		// Lazy init: the buffer is bound to a single property, so one template covers every
		// record. The benign race here is acceptable — both instances are equivalent because
		// PropertyGetterAccess is identified solely by Name; whichever assignment wins still
		// satisfies the contract. The template is only a Name source; per-record identity is
		// preserved by allocating a fresh PropertyGetterAccess in AppendBoxed.
		_access ??= new PropertyGetterAccess(name);
		Append();
	}

	/// <inheritdoc cref="IFastMemberBuffer.Clear" />
	public void Clear() => _storage.Clear();

	void IFastMemberBuffer.AppendBoxed(List<(long Seq, IInteraction Interaction)> dest)
	{
		lock (_storage.Lock)
		{
			int n = _storage.PublishedUnderLock;
			if (n == 0)
			{
				return;
			}

			string name = _access!.Name;
			for (int slot = 0; slot < n; slot++)
			{
				ref Record r = ref _storage.SlotUnderLock(slot);
				r.Boxed ??= new PropertyGetterAccess(name);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	void IFastMemberBuffer.AppendBoxedUnverified(List<(long Seq, IInteraction Interaction)> dest)
	{
		lock (_storage.Lock)
		{
			int n = _storage.PublishedUnderLock;
			if (n == 0)
			{
				return;
			}

			string name = _access!.Name;
			for (int slot = 0; slot < n; slot++)
			{
				if (_storage.VerifiedUnderLock(slot))
				{
					continue;
				}

				ref Record r = ref _storage.SlotUnderLock(slot);
				r.Boxed ??= new PropertyGetterAccess(name);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Returns the number of recorded property getter accesses and marks every currently-published
	///     slot as verified so a later <see cref="IMockInteractions.GetUnverifiedInteractions" /> walk
	///     skips them. The name reflects the side effect: this is a <c>Count</c> + <c>MarkVerified</c>
	///     step, exposed for symmetry with
	///     <see cref="FastPropertySetterBuffer{T}.ConsumeMatching(IParameterMatch{T})" />.
	/// </summary>
	public int ConsumeMatching()
	{
		lock (_storage.Lock)
		{
			int n = _storage.PublishedUnderLock;
			for (int slot = 0; slot < n; slot++)
			{
				_storage.VerifiedUnderLock(slot) = true;
			}

			return n;
		}
	}

	internal struct Record
	{
		public long Seq;
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
	private readonly ChunkedSlotStorage<Record> _storage = new();

	internal FastPropertySetterBuffer(FastMockInteractions owner)
	{
		_owner = owner;
	}

	/// <inheritdoc cref="IFastMemberBuffer.Count" />
	public int Count => _storage.Count;

	/// <summary>
	///     Records a property setter access.
	/// </summary>
	public void Append(string name, T value)
	{
		long seq = _owner.NextSequence();
		int slot = _storage.Reserve();
		ref Record r = ref _storage.SlotForWrite(slot);
		r.Seq = seq;
		r.Name = name;
		r.Value = value;
		r.Boxed = null;
		_storage.Publish();

		if (_owner.HasInteractionAddedSubscribers)
		{
			_owner.RaiseAdded();
		}
	}

	/// <inheritdoc cref="IFastMemberBuffer.Clear" />
	public void Clear() => _storage.Clear();

	void IFastMemberBuffer.AppendBoxed(List<(long Seq, IInteraction Interaction)> dest)
	{
		lock (_storage.Lock)
		{
			int n = _storage.PublishedUnderLock;
			for (int slot = 0; slot < n; slot++)
			{
				ref Record r = ref _storage.SlotUnderLock(slot);
				r.Boxed ??= new PropertySetterAccess<T>(r.Name, r.Value);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	void IFastMemberBuffer.AppendBoxedUnverified(List<(long Seq, IInteraction Interaction)> dest)
	{
		lock (_storage.Lock)
		{
			int n = _storage.PublishedUnderLock;
			for (int slot = 0; slot < n; slot++)
			{
				if (_storage.VerifiedUnderLock(slot))
				{
					continue;
				}

				ref Record r = ref _storage.SlotUnderLock(slot);
				r.Boxed ??= new PropertySetterAccess<T>(r.Name, r.Value);
				dest.Add((r.Seq, r.Boxed));
			}
		}
	}

	/// <summary>
	///     Counts recorded setter accesses whose assigned value satisfies <paramref name="match" />,
	///     marking each matched slot as verified so a later
	///     <see cref="IMockInteractions.GetUnverifiedInteractions" /> walk skips them. The name
	///     reflects the side effect: this is a <c>Count</c> + <c>MarkVerified</c> step, not a pure read.
	/// </summary>
	public int ConsumeMatching(IParameterMatch<T> match)
	{
		int matches = 0;
		lock (_storage.Lock)
		{
			int n = _storage.PublishedUnderLock;
			for (int slot = 0; slot < n; slot++)
			{
				ref Record r = ref _storage.SlotUnderLock(slot);
				if (match.Matches(r.Value))
				{
					matches++;
					_storage.VerifiedUnderLock(slot) = true;
				}
			}
		}

		return matches;
	}

	internal struct Record
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
	///     Creates and installs a property getter buffer at the given <paramref name="memberId" /> with
	///     a pre-built shared <paramref name="access" /> singleton. Used by the source generator so the
	///     buffer never has to allocate a <see cref="PropertyGetterAccess" /> on the first record or on
	///     verification.
	/// </summary>
	public static FastPropertyGetterBuffer InstallPropertyGetter(this FastMockInteractions interactions,
		int memberId, PropertyGetterAccess access)
	{
		FastPropertyGetterBuffer buffer = new(interactions, access);
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
