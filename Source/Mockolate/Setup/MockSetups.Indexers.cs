using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Mockolate.Interactions;

namespace Mockolate.Setup;

internal partial class MockSetups
{
	[DebuggerDisplay("{ToString()}")]
#if !DEBUG
	[DebuggerNonUserCode]
#endif
	internal sealed class IndexerSetups
	{
		private List<IndexerSetup>? _storage;
		private IndexerValueStorage?[]? _valueStorages;
		private readonly object _valueStoragesLock = new();

		// Member-id-indexed setup store. Populated alongside `_storage` under the same lock.
		// Mutable lists are kept for setup writes; `_snapshotByMemberId` is an immutable-per-generation
		// snapshot that the invocation hot path reads without acquiring the list lock.
		// `_hasStaleSnapshot` is set on every add; the next reader rebuilds the snapshot under the
		// lock. Same pattern as MockSetups.MethodSetups.
		private List<IndexerSetup>?[]? _byMemberId;
		private volatile IndexerSetup[]?[]? _snapshotByMemberId;
		private volatile bool _hasStaleSnapshot;

		public int Count
		{
			get
			{
				List<IndexerSetup>? storage = _storage;
				if (storage is null)
				{
					return 0;
				}

				lock (storage)
				{
					return storage.Count;
				}
			}
		}

		/// <summary>
		///     Pre-sizes the per-signature value-storage array. Optional — <see cref="GetOrCreateStorage{TValue}(int)" />
		///     grows lazily if unset. When the total signature count is known at mock-construction time, calling this
		///     once avoids any reallocations.
		/// </summary>
		internal void InitializeStorageCount(int count)
		{
			if (_valueStorages is null)
			{
				Interlocked.CompareExchange(ref _valueStorages, new IndexerValueStorage?[count], null);
			}
		}

		/// <summary>
		///     Returns the root value storage for the indexer signature at the given <paramref name="signatureIndex" />,
		///     creating one if none exists. Grows the backing array on demand when not pre-initialised.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="signatureIndex" /> is negative.</exception>
		internal IndexerValueStorage<TValue> GetOrCreateStorage<TValue>(int signatureIndex)
		{
			if (signatureIndex < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(signatureIndex), signatureIndex,
					"Signature index must be non-negative.");
			}

			IndexerValueStorage?[]? storages = _valueStorages;
			if (storages is null || storages.Length <= signatureIndex)
			{
				storages = EnsureCapacity(signatureIndex);
			}

			IndexerValueStorage? slot = storages[signatureIndex];
			if (slot is null)
			{
				IndexerValueStorage<TValue> created = new();
				slot = Interlocked.CompareExchange(ref storages[signatureIndex], created, null) ?? created;
			}

			if (slot is not IndexerValueStorage<TValue> typed)
			{
				throw new InvalidOperationException(
					$"Indexer storage at signature index {signatureIndex} was created as '{slot.GetType()}' but is being accessed as 'IndexerValueStorage<{typeof(TValue)}>'. This indicates a signature-index collision between distinct indexer signatures - please report a bug against the source generator.");
			}

			return typed;
		}

		private IndexerValueStorage?[] EnsureCapacity(int signatureIndex)
		{
			lock (_valueStoragesLock)
			{
				IndexerValueStorage?[]? storages = _valueStorages;
				if (storages is null)
				{
					storages = new IndexerValueStorage?[signatureIndex + 1];
					_valueStorages = storages;
					return storages;
				}

				if (storages.Length <= signatureIndex)
				{
					IndexerValueStorage?[] grown = new IndexerValueStorage?[signatureIndex + 1];
					Array.Copy(storages, grown, storages.Length);
					_valueStorages = grown;
					return grown;
				}

				return storages;
			}
		}

		private List<IndexerSetup> GetOrCreateStorage()
		{
			if (_storage is null)
			{
				Interlocked.CompareExchange(ref _storage, [], null);
			}

			return _storage!;
		}

		public T? GetMatching<T>(Func<T, bool> predicate) where T : IndexerSetup
		{
			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					if (storage[i] is T typedSetup && predicate(typedSetup))
					{
						return typedSetup;
					}
				}
			}

			return null;
		}

		public T? GetMatching<T>(IndexerAccess access) where T : IndexerSetup
		{
			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					if (storage[i] is T typedSetup &&
					    ((IInteractiveIndexerSetup)typedSetup).Matches(access))
					{
						return typedSetup;
					}
				}
			}

			return null;
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			List<IndexerSetup>? storage = _storage;
			if (storage is null || storage.Count == 0)
			{
				return "0 indexers";
			}

			lock (storage)
			{
				int count = storage.Count;
				StringBuilder sb = new();
				sb.Append(count).Append(count == 1 ? " indexer:" : " indexers:").AppendLine();
				foreach (IndexerSetup indexerSetup in storage)
				{
					sb.Append(indexerSetup).AppendLine();
				}

				sb.Length -= Environment.NewLine.Length;
				return sb.ToString();
			}
		}

		internal void Add(IndexerSetup setup)
		{
			List<IndexerSetup> storage = GetOrCreateStorage();
			lock (storage)
			{
				storage.Add(setup);
				int memberId = setup.MemberId;
				if (memberId >= 0)
				{
					EnsureByMemberIdCapacity(memberId);
					List<IndexerSetup>? list = _byMemberId![memberId];
					if (list is null)
					{
						list = new List<IndexerSetup>();
						_byMemberId[memberId] = list;
					}

					list.Add(setup);
					_hasStaleSnapshot = true;
				}
			}
		}

		private void EnsureByMemberIdCapacity(int memberId)
		{
			int required = memberId + 1;
			if (_byMemberId is null)
			{
				int initial = Math.Max(required, 8);
				_byMemberId = new List<IndexerSetup>?[initial];
				return;
			}

			if (_byMemberId.Length >= required)
			{
				return;
			}

			int newSize = Math.Max(required, _byMemberId.Length * 2);
			List<IndexerSetup>?[] grown = new List<IndexerSetup>?[newSize];
			Array.Copy(_byMemberId, grown, _byMemberId.Length);
			_byMemberId = grown;
		}

		private IndexerSetup[]?[]? GetSnapshot()
		{
			IndexerSetup[]?[]? current = _snapshotByMemberId;
			if (!_hasStaleSnapshot && current is not null)
			{
				return current;
			}

			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				if (!_hasStaleSnapshot)
				{
					return _snapshotByMemberId;
				}

				List<IndexerSetup>?[]? byMemberId = _byMemberId;
				if (byMemberId is null)
				{
					_snapshotByMemberId = null;
					_hasStaleSnapshot = false;
					return null;
				}

				IndexerSetup[]?[] rebuilt = new IndexerSetup[]?[byMemberId.Length];
				for (int i = 0; i < byMemberId.Length; i++)
				{
					List<IndexerSetup>? list = byMemberId[i];
					if (list is { Count: > 0, })
					{
						rebuilt[i] = list.ToArray();
					}
				}

				_snapshotByMemberId = rebuilt;
				_hasStaleSnapshot = false;
				return rebuilt;
			}
		}

		/// <summary>
		///     O(1) getter-setup lookup by generator-assigned member id for single-argument indexers.
		///     Returns the latest-registered setup of type <typeparamref name="T" /> whose
		///     <see cref="IIndexerGetterMatchByValue{T1}" /> accepts <paramref name="arg1" />, or
		///     <see langword="null" /> when none matches. Invocation hot path — reads the volatile
		///     snapshot without a lock. Falls back to a type-filtered scan of legacy setups
		///     (<see cref="IndexerSetup.MemberId" /> &lt; 0).
		/// </summary>
		public T? GetGetterByMemberId<T, T1>(int memberId, T1 arg1) where T : IndexerSetup
		{
			IndexerSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				IndexerSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						IndexerSetup setup = setups[i];
						if (setup is T typed && setup is IIndexerGetterMatchByValue<T1> m && m.Matches(arg1))
						{
							return typed;
						}
					}
				}
			}

			return GetLegacyGetterByValue<T, T1>(arg1);
		}

		/// <summary>
		///     O(1) getter-setup lookup by member id for two-argument indexers.
		/// </summary>
		public T? GetGetterByMemberId<T, T1, T2>(int memberId, T1 arg1, T2 arg2) where T : IndexerSetup
		{
			IndexerSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				IndexerSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						IndexerSetup setup = setups[i];
						if (setup is T typed && setup is IIndexerGetterMatchByValue<T1, T2> m
						    && m.Matches(arg1, arg2))
						{
							return typed;
						}
					}
				}
			}

			return GetLegacyGetterByValue<T, T1, T2>(arg1, arg2);
		}

		/// <summary>
		///     O(1) getter-setup lookup by member id for three-argument indexers.
		/// </summary>
		public T? GetGetterByMemberId<T, T1, T2, T3>(int memberId, T1 arg1, T2 arg2, T3 arg3)
			where T : IndexerSetup
		{
			IndexerSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				IndexerSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						IndexerSetup setup = setups[i];
						if (setup is T typed && setup is IIndexerGetterMatchByValue<T1, T2, T3> m
						    && m.Matches(arg1, arg2, arg3))
						{
							return typed;
						}
					}
				}
			}

			return GetLegacyGetterByValue<T, T1, T2, T3>(arg1, arg2, arg3);
		}

		/// <summary>
		///     O(1) getter-setup lookup by member id for four-argument indexers.
		/// </summary>
		public T? GetGetterByMemberId<T, T1, T2, T3, T4>(int memberId, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
			where T : IndexerSetup
		{
			IndexerSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				IndexerSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						IndexerSetup setup = setups[i];
						if (setup is T typed && setup is IIndexerGetterMatchByValue<T1, T2, T3, T4> m
						    && m.Matches(arg1, arg2, arg3, arg4))
						{
							return typed;
						}
					}
				}
			}

			return GetLegacyGetterByValue<T, T1, T2, T3, T4>(arg1, arg2, arg3, arg4);
		}

		/// <summary>
		///     O(1) setter-setup lookup by generator-assigned member id for single-argument indexers.
		/// </summary>
		public T? GetSetterByMemberId<T, T1, TValue>(int memberId, T1 arg1, TValue value)
			where T : IndexerSetup
		{
			IndexerSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				IndexerSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						IndexerSetup setup = setups[i];
						if (setup is T typed && setup is IIndexerSetterMatchByValue<T1, TValue> m
						    && m.Matches(arg1, value))
						{
							return typed;
						}
					}
				}
			}

			return GetLegacySetterByValue<T, T1, TValue>(arg1, value);
		}

		/// <summary>
		///     O(1) setter-setup lookup by member id for two-argument indexers.
		/// </summary>
		public T? GetSetterByMemberId<T, T1, T2, TValue>(int memberId, T1 arg1, T2 arg2, TValue value)
			where T : IndexerSetup
		{
			IndexerSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				IndexerSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						IndexerSetup setup = setups[i];
						if (setup is T typed && setup is IIndexerSetterMatchByValue<T1, T2, TValue> m
						    && m.Matches(arg1, arg2, value))
						{
							return typed;
						}
					}
				}
			}

			return GetLegacySetterByValue<T, T1, T2, TValue>(arg1, arg2, value);
		}

		/// <summary>
		///     O(1) setter-setup lookup by member id for three-argument indexers.
		/// </summary>
		public T? GetSetterByMemberId<T, T1, T2, T3, TValue>(int memberId, T1 arg1, T2 arg2, T3 arg3, TValue value)
			where T : IndexerSetup
		{
			IndexerSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				IndexerSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						IndexerSetup setup = setups[i];
						if (setup is T typed && setup is IIndexerSetterMatchByValue<T1, T2, T3, TValue> m
						    && m.Matches(arg1, arg2, arg3, value))
						{
							return typed;
						}
					}
				}
			}

			return GetLegacySetterByValue<T, T1, T2, T3, TValue>(arg1, arg2, arg3, value);
		}

		/// <summary>
		///     O(1) setter-setup lookup by member id for four-argument indexers.
		/// </summary>
		public T? GetSetterByMemberId<T, T1, T2, T3, T4, TValue>(int memberId, T1 arg1, T2 arg2, T3 arg3, T4 arg4,
			TValue value) where T : IndexerSetup
		{
			IndexerSetup[]?[]? snap = GetSnapshot();
			if (snap is not null && (uint)memberId < (uint)snap.Length)
			{
				IndexerSetup[]? setups = snap[memberId];
				if (setups is not null)
				{
					for (int i = setups.Length - 1; i >= 0; i--)
					{
						IndexerSetup setup = setups[i];
						if (setup is T typed && setup is IIndexerSetterMatchByValue<T1, T2, T3, T4, TValue> m
						    && m.Matches(arg1, arg2, arg3, arg4, value))
						{
							return typed;
						}
					}
				}
			}

			return GetLegacySetterByValue<T, T1, T2, T3, T4, TValue>(arg1, arg2, arg3, arg4, value);
		}

		// Legacy fallback: setups registered with the pre-memberId ctor (MemberId < 0) live only in
		// `_storage`. Scan them under the list lock, newest first. The `setup is T` test filters by
		// the concrete IndexerSetup<TValue, T1..> shape, so we don't need a name to disambiguate.
		private T? GetLegacyGetterByValue<T, T1>(T1 arg1) where T : IndexerSetup
		{
			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					IndexerSetup setup = storage[i];
					if (setup.MemberId >= 0)
					{
						continue;
					}

					if (setup is T typed && setup is IIndexerGetterMatchByValue<T1> m && m.Matches(arg1))
					{
						return typed;
					}
				}
			}

			return null;
		}

		private T? GetLegacyGetterByValue<T, T1, T2>(T1 arg1, T2 arg2) where T : IndexerSetup
		{
			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					IndexerSetup setup = storage[i];
					if (setup.MemberId >= 0)
					{
						continue;
					}

					if (setup is T typed && setup is IIndexerGetterMatchByValue<T1, T2> m && m.Matches(arg1, arg2))
					{
						return typed;
					}
				}
			}

			return null;
		}

		private T? GetLegacyGetterByValue<T, T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3) where T : IndexerSetup
		{
			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					IndexerSetup setup = storage[i];
					if (setup.MemberId >= 0)
					{
						continue;
					}

					if (setup is T typed && setup is IIndexerGetterMatchByValue<T1, T2, T3> m
					    && m.Matches(arg1, arg2, arg3))
					{
						return typed;
					}
				}
			}

			return null;
		}

		private T? GetLegacyGetterByValue<T, T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
			where T : IndexerSetup
		{
			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					IndexerSetup setup = storage[i];
					if (setup.MemberId >= 0)
					{
						continue;
					}

					if (setup is T typed && setup is IIndexerGetterMatchByValue<T1, T2, T3, T4> m
					    && m.Matches(arg1, arg2, arg3, arg4))
					{
						return typed;
					}
				}
			}

			return null;
		}

		private T? GetLegacySetterByValue<T, T1, TValue>(T1 arg1, TValue value) where T : IndexerSetup
		{
			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					IndexerSetup setup = storage[i];
					if (setup.MemberId >= 0)
					{
						continue;
					}

					if (setup is T typed && setup is IIndexerSetterMatchByValue<T1, TValue> m
					    && m.Matches(arg1, value))
					{
						return typed;
					}
				}
			}

			return null;
		}

		private T? GetLegacySetterByValue<T, T1, T2, TValue>(T1 arg1, T2 arg2, TValue value)
			where T : IndexerSetup
		{
			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					IndexerSetup setup = storage[i];
					if (setup.MemberId >= 0)
					{
						continue;
					}

					if (setup is T typed && setup is IIndexerSetterMatchByValue<T1, T2, TValue> m
					    && m.Matches(arg1, arg2, value))
					{
						return typed;
					}
				}
			}

			return null;
		}

		private T? GetLegacySetterByValue<T, T1, T2, T3, TValue>(T1 arg1, T2 arg2, T3 arg3, TValue value)
			where T : IndexerSetup
		{
			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					IndexerSetup setup = storage[i];
					if (setup.MemberId >= 0)
					{
						continue;
					}

					if (setup is T typed && setup is IIndexerSetterMatchByValue<T1, T2, T3, TValue> m
					    && m.Matches(arg1, arg2, arg3, value))
					{
						return typed;
					}
				}
			}

			return null;
		}

		private T? GetLegacySetterByValue<T, T1, T2, T3, T4, TValue>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, TValue value)
			where T : IndexerSetup
		{
			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return null;
			}

			lock (storage)
			{
				for (int i = storage.Count - 1; i >= 0; i--)
				{
					IndexerSetup setup = storage[i];
					if (setup.MemberId >= 0)
					{
						continue;
					}

					if (setup is T typed && setup is IIndexerSetterMatchByValue<T1, T2, T3, T4, TValue> m
					    && m.Matches(arg1, arg2, arg3, arg4, value))
					{
						return typed;
					}
				}
			}

			return null;
		}

		internal IEnumerable<IndexerSetup> EnumerateUnusedSetupsBy(MockInteractions interactions)
		{
			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return [];
			}

			lock (storage)
			{
				return storage.Where(indexerSetup => interactions.OfType<IndexerAccess>()
						.All(indexerAccess => !((IInteractiveIndexerSetup)indexerSetup).Matches(indexerAccess)))
					.ToList();
			}
		}
	}
}
