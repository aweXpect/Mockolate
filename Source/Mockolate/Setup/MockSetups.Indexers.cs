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
		internal IndexerValueStorage<TValue> GetOrCreateStorage<TValue>(int signatureIndex)
		{
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

			return (IndexerValueStorage<TValue>)slot;
		}

		private IndexerValueStorage?[] EnsureCapacity(int signatureIndex)
		{
			lock (this)
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
			}
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
