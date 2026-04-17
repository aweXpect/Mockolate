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
		private ValueStorage? _valueStorage;

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
		///     The root value storage used to track stored indexer values (keyed by parameter tree).
		/// </summary>
		public ValueStorage ValueStorage
		{
			get
			{
				if (_valueStorage is null)
				{
					Interlocked.CompareExchange(ref _valueStorage, new ValueStorage(), null);
				}

				return _valueStorage!;
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

/// <summary>
///     Object-keyed tree used to store indexer values by their typed parameter path.
/// </summary>
[DebuggerNonUserCode]
internal sealed class ValueStorage
{
	private readonly List<KeyValueEntry> _storage = [];

	/// <summary>
	///     The value stored at the current tree node.
	/// </summary>
	public object? Value { get; set; }

	/// <summary>
	///     Returns the child storage for the given <paramref name="key" />, or <see langword="null" /> if none exists.
	/// </summary>
	public ValueStorage? GetChild(object? key)
	{
		lock (_storage)
		{
			foreach (KeyValueEntry entry in _storage)
			{
				if (Equals(entry.Key, key))
				{
					return entry.Value;
				}
			}
		}

		return null;
	}

	/// <summary>
	///     Returns the child storage for the given <paramref name="key" />, or creates one if none exists.
	/// </summary>
	public ValueStorage GetOrAddChild(object? key)
	{
		lock (_storage)
		{
			foreach (KeyValueEntry entry in _storage)
			{
				if (Equals(entry.Key, key))
				{
					return entry.Value;
				}
			}

			ValueStorage newValue = new();
			_storage.Add(new KeyValueEntry(key, newValue));
			return newValue;
		}
	}

	private readonly struct KeyValueEntry(object? key, ValueStorage value)
	{
		internal object? Key { get; } = key;
		internal ValueStorage Value { get; } = value;
	}
}
