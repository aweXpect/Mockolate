using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Setup;

[DebuggerDisplay("{ToString()}")]
[DebuggerNonUserCode]
internal class MockSetups
{
	internal EventSetups Events { get; } = new();
	internal IndexerSetups Indexers { get; } = new();
	internal MethodSetups Methods { get; } = new();
	internal PropertySetups Properties { get; } = new();
	
	/// <inheritdoc cref="object.ToString()" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		StringBuilder sb = new();
		if (Methods.Count > 0)
		{
			sb.Append(Methods.Count).Append(Methods.Count == 1 ? " method, " : " methods, ");
		}

		if (Properties.Count > 0)
		{
			sb.Append(Properties.Count).Append(Properties.Count == 1 ? " property, " : " properties, ");
		}

		if (Indexers.Count > 0)
		{
			sb.Append(Indexers.Count).Append(Indexers.Count == 1 ? " indexer, " : " indexers, ");
		}

		if (Events.Count > 0)
		{
			sb.Append(Events.Count).Append(Events.Count == 1 ? " event, " : " events, ");
		}

		if (sb.Length == 0)
		{
			return "no setups";
		}

		sb.Length -= 2;
		return sb.ToString();
	}

	[DebuggerDisplay("{ToString()}")]
	[DebuggerNonUserCode]
	internal sealed class MethodSetups
	{
		private ConcurrentStack<MethodSetup>? _storage;

		public int Count
			=> _storage?.Count ?? 0;

		public void Add(MethodSetup setup)
		{
			_storage ??= new ConcurrentStack<MethodSetup>();
			_storage.Push(setup);
		}

		public MethodSetup? GetLatestOrDefault(Func<MethodSetup, bool> predicate)
		{
			if (_storage is null)
			{
				return null;
			}

			return _storage.FirstOrDefault(predicate);
		}

		internal IEnumerable<MethodSetup> EnumerateUnusedSetupsBy(MockInteractions interactions)
		{
			if (_storage is null)
			{
				return [];
			}

			return _storage.Where(methodSetup => interactions.Interactions.OfType<MethodInvocation>()
				.All(methodInvocation => !((IInteractiveMethodSetup)methodSetup).Matches(methodInvocation)));
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			if (_storage is null || _storage.IsEmpty)
			{
				return "0 methods";
			}

			StringBuilder sb = new();
			sb.Append(_storage.Count).Append(_storage.Count == 1 ? " method:" : " methods:").AppendLine();
			foreach (MethodSetup methodSetup in _storage)
			{
				sb.Append(methodSetup).AppendLine();
			}

			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}
	}

	[DebuggerDisplay("{ToString()}")]
	[DebuggerNonUserCode]
	internal sealed class PropertySetups
	{
		private ConcurrentDictionary<string, PropertySetup>? _storage;

		public int Count { get; private set; }

		public void Add(PropertySetup setup)
		{
			_storage ??= new ConcurrentDictionary<string, PropertySetup>();
			_storage.AddOrUpdate(setup.Name, setup, (_, _) => setup);
			Count = _storage.Count(NotDefaultPredicate);

			[DebuggerNonUserCode]
			static bool NotDefaultPredicate(KeyValuePair<string, PropertySetup> x)
			{
				return x.Value is not PropertySetup.Default;
			}
		}

		public bool TryGetValue(string propertyName, [NotNullWhen(true)] out PropertySetup? setup)
		{
			_storage ??= new ConcurrentDictionary<string, PropertySetup>();
			return _storage.TryGetValue(propertyName, out setup);
		}

		internal IEnumerable<PropertySetup> EnumerateUnusedSetupsBy(MockInteractions interactions)
		{
			if (_storage is null)
			{
				return [];
			}

			return _storage.Values.Where(propertySetup => interactions.Interactions.OfType<PropertyAccess>()
				.All(propertyAccess => !((IInteractivePropertySetup)propertySetup).Matches(propertyAccess)));
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			if (_storage is null || _storage.IsEmpty)
			{
				return "0 properties";
			}

			List<KeyValuePair<string, PropertySetup>> setups =
				_storage.Where(x => x.Value is not PropertySetup.Default).ToList();

			if (setups.Count == 0)
			{
				return "0 properties";
			}

			StringBuilder sb = new();
			sb.Append(setups.Count).Append(setups.Count == 1 ? " property:" : " properties:").AppendLine();
			foreach (KeyValuePair<string, PropertySetup> item in setups)
			{
				sb.Append(item.Value).Append(' ').Append(item.Key).AppendLine();
			}

			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}
	}

	[DebuggerDisplay("{ToString()}")]
	[DebuggerNonUserCode]
	internal sealed class IndexerSetups
	{
		private ConcurrentStack<IndexerSetup>? _storage;

		private ValueStorage? _valueStorage;

		public int Count => _storage?.Count ?? 0;

		public IndexerSetup? GetLastestOrDefault(Func<IndexerSetup, bool> predicate)
		{
			if (_storage is null)
			{
				return null;
			}

			return _storage.FirstOrDefault(predicate);
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			if (_storage is null || Count == 0)
			{
				return "0 indexers";
			}

			StringBuilder sb = new();
			sb.Append(Count).Append(Count == 1 ? " indexer:" : " indexers:").AppendLine();
			foreach (IndexerSetup indexerSetup in _storage)
			{
				sb.Append(indexerSetup).AppendLine();
			}

			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}

		internal void Add(IndexerSetup setup)
		{
			_storage ??= new ConcurrentStack<IndexerSetup>();
			_storage.Push(setup);
		}

		internal TValue GetOrAddValue<TValue>(NamedParameterValue[] parameters, Func<TValue> valueGenerator)
		{
			_valueStorage ??= new ValueStorage();
			ValueStorage? storage = _valueStorage;
			foreach (NamedParameterValue parameter in parameters)
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

		internal void UpdateValue<TValue>(NamedParameterValue[] parameters, TValue value)
		{
			_valueStorage ??= new ValueStorage();
			ValueStorage? storage = _valueStorage;
			foreach (NamedParameterValue parameter in parameters)
			{
				storage = storage.GetOrAdd(parameter, () => new ValueStorage());
			}

			storage.Value = value;
		}

		internal IEnumerable<IndexerSetup> EnumerateUnusedSetupsBy(MockInteractions interactions)
		{
			if (_storage is null)
			{
				return [];
			}

			return _storage.Where(indexerSetup => interactions.Interactions.OfType<IndexerAccess>()
				.All(indexerAccess => !((IInteractiveIndexerSetup)indexerSetup).Matches(indexerAccess)));
		}

		[DebuggerNonUserCode]
		private sealed class ValueStorage
		{
			private ConcurrentDictionary<NamedParameterValue, ValueStorage>? _storage;

			public object? Value { get; set; }

			public ValueStorage GetOrAdd(NamedParameterValue key, Func<ValueStorage> valueGenerator)
			{
				_storage ??=
					new ConcurrentDictionary<NamedParameterValue, ValueStorage>(NamedParameterValueComparer.Instance);
				return _storage.GetOrAdd(key, _ => valueGenerator());
			}
		}

		[ExcludeFromCodeCoverage]
		[DebuggerNonUserCode]
		private sealed class NamedParameterValueComparer : IEqualityComparer<NamedParameterValue>
		{
			public static readonly NamedParameterValueComparer Instance = new();

			public bool Equals(NamedParameterValue x, NamedParameterValue y)
				=> string.Equals(x.Name, y.Name, StringComparison.Ordinal)
				   && (ReferenceEquals(x.Value, y.Value) || (x.Value?.Equals(y.Value) ?? y.Value is null));

			public int GetHashCode(NamedParameterValue obj)
			{
				int hash = 17;
				hash = (hash * 31) + (obj.Name is not null ? StringComparer.Ordinal.GetHashCode(obj.Name) : 0);
				hash = (hash * 31) + (obj.Value?.GetHashCode() ?? 0);
				return hash;
			}
		}
	}

	[DebuggerDisplay("{ToString()}")]
	[DebuggerNonUserCode]
	internal sealed class EventSetups
	{
		/// <summary>
		///     This value is only needed, to use the <see cref="ConcurrentDictionary{TKey, TValue}" /> as a
		///     thread-safe collection of keys, because .NET does not have a thread-safe <see cref="HashSet{T}" />.
		/// </summary>
		private const bool Value = true;

		private ConcurrentDictionary<(object?, MethodInfo, string), bool>? _storage;

		public int Count
			=> _storage?.Count ?? 0;

		public IEnumerable<(object?, MethodInfo, string)> Enumerate()
		{
			if (_storage is null)
			{
				yield break;
			}

			foreach ((object?, MethodInfo, string) item in _storage.Keys)
			{
				yield return item;
			}
		}

		public void Add(object? target, MethodInfo method, string eventName)
		{
			_storage ??= new ConcurrentDictionary<(object?, MethodInfo, string), bool>();
			_storage.TryAdd((target, method, eventName), Value);
		}

		public void Remove(object? target, MethodInfo method, string eventName)
		{
			_storage ??= new ConcurrentDictionary<(object?, MethodInfo, string), bool>();
			_storage.TryRemove((target, method, eventName), out _);
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			if (_storage is null || _storage.IsEmpty)
			{
				return "0 events";
			}

			StringBuilder sb = new();
			sb.Append(_storage.Count).Append(_storage.Count == 1 ? " event:" : " events:").AppendLine();
			foreach ((object?, MethodInfo, string) item in _storage.Keys)
			{
				sb.Append(item.Item3).AppendLine();
			}

			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}
	}
}
