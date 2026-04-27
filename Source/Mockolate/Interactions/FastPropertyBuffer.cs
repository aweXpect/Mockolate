using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mockolate.Parameters;

namespace Mockolate.Interactions;

/// <summary>
///     Per-member buffer for property getters. The buffer is bound to a single property; every
///     recorded record stores only a sequence number and shares a single
///     <see cref="PropertyGetterAccess" /> when boxed for verification. Sharing one reference
///     across records is safe because property getters carry no parameters — every recorded
///     access is semantically identical — and the two reference-keyed verification paths in
///     this codebase tolerate it: <c>FastMockInteractions._verified</c> filters all-or-nothing
///     per matched property, and <c>VerificationResultExtensions.Then</c> walks the snapshot
///     positionally so repeated occurrences of the same reference still resolve to distinct
///     positions.
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
	///     <see cref="PropertyGetterAccess" /> singleton. Throws when the singleton has not been
	///     installed — callers must use <see cref="Append(string)" /> in that case.
	/// </summary>
	/// <exception cref="InvalidOperationException">No singleton was supplied at install time.</exception>
	public void Append()
	{
		if (_access is null)
		{
			throw new InvalidOperationException(
				$"{nameof(Append)}() requires the buffer to be installed with a {nameof(PropertyGetterAccess)} singleton via {nameof(FastPropertyBufferFactory)}.{nameof(FastPropertyBufferFactory.InstallPropertyGetter)}(memberId, access). Use {nameof(Append)}(string) when no singleton is available.");
		}

		long seq = _owner.NextSequence();
		int slot = _storage.Reserve();
		ref Record r = ref _storage.SlotForWrite(slot);
		r.Seq = seq;
		_storage.Publish();

		if (_owner.HasInteractionAddedSubscribers)
		{
			_owner.RaiseAdded();
		}
	}

	/// <summary>
	///     Records a property getter access. Lazily installs the buffer's
	///     <see cref="PropertyGetterAccess" /> singleton from <paramref name="name" /> on first
	///     call so legacy callers (generated code that does not pass a pre-built singleton) keep
	///     working without allocating one access object per record.
	/// </summary>
	public void Append(string name)
	{
		// Lazy init: the buffer is bound to a single property, so one singleton covers every
		// record. The benign race here is acceptable — both instances are equivalent because
		// PropertyGetterAccess is identified solely by Name; whichever assignment wins still
		// satisfies the contract.
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

			PropertyGetterAccess access = _access!;
			for (int slot = 0; slot < n; slot++)
			{
				ref Record r = ref _storage.SlotUnderLock(slot);
				dest.Add((r.Seq, access));
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

			PropertyGetterAccess access = _access!;
			for (int slot = 0; slot < n; slot++)
			{
				if (_storage.VerifiedUnderLock(slot))
				{
					continue;
				}

				ref Record r = ref _storage.SlotUnderLock(slot);
				dest.Add((r.Seq, access));
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
