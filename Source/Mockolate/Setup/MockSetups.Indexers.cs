using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Setup;

internal partial class MockSetups
{
	internal IndexerSetups Indexers { get; } = new();

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

		private List<IndexerSetup> GetOrCreateStorage()
		{
			if (_storage is null)
			{
				Interlocked.CompareExchange(ref _storage, [], null);
			}

			return _storage!;
		}

		public IndexerSetup? GetLatestOrDefault(IndexerAccess interaction)
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
					if (((IInteractiveIndexerSetup)storage[i]).Matches(interaction))
					{
						return storage[i];
					}
				}
			}

			return null;
		}

		public IndexerSetup? GetLatestOrDefault(Predicate<IndexerSetup> predicate)
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
					if (predicate(storage[i]))
					{
						return storage[i];
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

		internal TValue GetOrAddValue<TValue>(INamedParameterValue[] parameters, Func<TValue> valueGenerator)
		{
			_valueStorage ??= new ValueStorage();
			ValueStorage? storage = _valueStorage;
			foreach (INamedParameterValue parameter in parameters)
			{
				storage = storage.GetOrAdd(parameter, () => new ValueStorage());
			}

			if (storage.Value is TValue value)
			{
				return value;
			}

			value = valueGenerator();
			storage.Value = value;
			return value;
		}

		internal void UpdateValue<TValue>(INamedParameterValue[] parameters, TValue value)
		{
			_valueStorage ??= new ValueStorage();
			ValueStorage? storage = _valueStorage;
			foreach (INamedParameterValue parameter in parameters)
			{
				storage = storage.GetOrAdd(parameter, () => new ValueStorage());
			}

			storage.Value = value;
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

		[DebuggerNonUserCode]
		private sealed class ValueStorage
		{
			private readonly List<(INamedParameterValue Key, ValueStorage Value)> _storage = [];

			public object? Value { get; set; }

			public ValueStorage GetOrAdd(INamedParameterValue key, Func<ValueStorage> valueGenerator)
			{
				lock (_storage)
				{
					foreach ((INamedParameterValue existingKey, ValueStorage existingValue) in _storage)
					{
						if (existingKey.Equals(key))
						{
							return existingValue;
						}
					}

					ValueStorage newValue = valueGenerator();
					_storage.Add((key, newValue));
					return newValue;
				}
			}
		}
	}
}
