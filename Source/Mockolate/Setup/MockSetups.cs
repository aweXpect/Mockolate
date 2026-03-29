using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
		int methodCount = Methods.Count;
		if (methodCount > 0)
		{
			sb.Append(methodCount).Append(methodCount == 1 ? " method, " : " methods, ");
		}

		int propertyCount = Properties.Count;
		if (propertyCount > 0)
		{
			sb.Append(propertyCount).Append(propertyCount == 1 ? " property, " : " properties, ");
		}

		int indexerCount = Indexers.Count;
		if (indexerCount > 0)
		{
			sb.Append(indexerCount).Append(indexerCount == 1 ? " indexer, " : " indexers, ");
		}

		int eventCount = Events.Count;
		if (eventCount > 0)
		{
			sb.Append(eventCount).Append(eventCount == 1 ? " event, " : " events, ");
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
		private List<MethodSetup>? _storage;

		public int Count
		{
			get
			{
				List<MethodSetup>? storage = _storage;
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

		private List<MethodSetup> GetOrCreateStorage()
		{
			if (_storage is null)
			{
				Interlocked.CompareExchange(ref _storage, [], null);
			}

			return _storage!;
		}

		public void Add(MethodSetup setup)
		{
			List<MethodSetup> storage = GetOrCreateStorage();
			lock (storage)
			{
				storage.Add(setup);
			}
		}

		public MethodSetup? GetLatestOrDefault(Func<MethodSetup, bool> predicate)
		{
			List<MethodSetup>? storage = _storage;
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

		internal IEnumerable<MethodSetup> EnumerateUnusedSetupsBy(MockInteractions interactions)
		{
			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				return [];
			}

			lock (storage)
			{
				return storage.Where(methodSetup => interactions.Interactions.OfType<MethodInvocation>()
						.All(methodInvocation => !((IInteractiveMethodSetup)methodSetup).Matches(methodInvocation)))
					.ToList();
			}
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			List<MethodSetup>? storage = _storage;
			if (storage is null)
			{
				return "0 methods";
			}

			lock (storage)
			{
				int count = storage.Count;
				if (count == 0)
				{
					return "0 methods";
				}

				StringBuilder sb = new();
				sb.Append(count).Append(count == 1 ? " method:" : " methods:").AppendLine();
				foreach (MethodSetup methodSetup in storage)
				{
					sb.Append(methodSetup).AppendLine();
				}

				sb.Length -= Environment.NewLine.Length;
				return sb.ToString();
			}
		}
	}

	[DebuggerDisplay("{ToString()}")]
	[DebuggerNonUserCode]
	internal sealed class PropertySetups
	{
		private int _count;
		private List<PropertySetup>? _storage;

		// ReSharper disable once InconsistentlySynchronizedField
		public int Count => Volatile.Read(ref _count);

		private List<PropertySetup> GetOrCreateStorage()
		{
			if (_storage is null)
			{
				Interlocked.CompareExchange(ref _storage, [], null);
			}

			return _storage!;
		}

		public void Add(PropertySetup setup)
		{
			List<PropertySetup> storage = GetOrCreateStorage();
			lock (storage)
			{
				for (int i = 0; i < storage.Count; i++)
				{
					if (string.Equals(storage[i].Name, setup.Name, StringComparison.Ordinal))
					{
						bool wasDefault = storage[i] is PropertySetup.Default;
						bool isDefault = setup is PropertySetup.Default;
						storage[i] = setup;
						if (wasDefault && !isDefault)
						{
							Volatile.Write(ref _count, _count + 1);
						}
						else if (!wasDefault && isDefault)
						{
							Volatile.Write(ref _count, _count - 1);
						}

						return;
					}
				}

				storage.Add(setup);
				if (setup is not PropertySetup.Default)
				{
					Volatile.Write(ref _count, _count + 1);
				}
			}
		}

		public bool TryGetValue(string propertyName, [NotNullWhen(true)] out PropertySetup? setup)
		{
			List<PropertySetup>? storage = _storage;
			if (storage is not null)
			{
				lock (storage)
				{
					for (int i = 0; i < storage.Count; i++)
					{
						if (string.Equals(storage[i].Name, propertyName, StringComparison.Ordinal))
						{
							setup = storage[i];
							return true;
						}
					}
				}
			}

			setup = null;
			return false;
		}

		internal IEnumerable<PropertySetup> EnumerateUnusedSetupsBy(MockInteractions interactions)
		{
			List<PropertySetup>? storage = _storage;
			if (storage is null)
			{
				return [];
			}

			lock (storage)
			{
				return storage.Where(propertySetup => interactions.Interactions.OfType<PropertyAccess>()
						.All(propertyAccess => !((IInteractivePropertySetup)propertySetup).Matches(propertyAccess)))
					.ToList();
			}
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			List<PropertySetup>? storage = _storage;
			if (storage is null || storage.Count == 0)
			{
				return "0 properties";
			}

			lock (storage)
			{
				List<PropertySetup>? setups = null;
				foreach (PropertySetup setup in storage)
				{
					if (setup is not PropertySetup.Default)
					{
						setups ??= [];
						setups.Add(setup);
					}
				}

				if (setups is null || setups.Count == 0)
				{
					return "0 properties";
				}

				StringBuilder sb = new();
				sb.Append(setups.Count).Append(setups.Count == 1 ? " property:" : " properties:").AppendLine();
				foreach (PropertySetup item in setups)
				{
					sb.Append(item).Append(' ').Append(item.Name).AppendLine();
				}

				sb.Length -= Environment.NewLine.Length;
				return sb.ToString();
			}
		}
	}

	[DebuggerDisplay("{ToString()}")]
	[DebuggerNonUserCode]
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
			List<IndexerSetup>? storage = _storage;
			if (storage is null)
			{
				return [];
			}

			lock (storage)
			{
				return storage.Where(indexerSetup => interactions.Interactions.OfType<IndexerAccess>()
						.All(indexerAccess => !((IInteractiveIndexerSetup)indexerSetup).Matches(indexerAccess)))
					.ToList();
			}
		}

		[DebuggerNonUserCode]
		private sealed class ValueStorage
		{
			private readonly List<(NamedParameterValue Key, ValueStorage Value)> _storage = [];

			public object? Value { get; set; }

			public ValueStorage GetOrAdd(NamedParameterValue key, Func<ValueStorage> valueGenerator)
			{
				lock (_storage)
				{
					foreach ((NamedParameterValue existingKey, ValueStorage existingValue) in _storage)
					{
						if (KeyEquals(existingKey, key))
						{
							return existingValue;
						}
					}

					ValueStorage newValue = valueGenerator();
					_storage.Add((key, newValue));
					return newValue;
				}
			}

			private static bool KeyEquals(NamedParameterValue x, NamedParameterValue y)
				=> string.Equals(x.Name, y.Name, StringComparison.Ordinal)
				   && (ReferenceEquals(x.Value, y.Value) || (x.Value?.Equals(y.Value) ?? y.Value is null));
		}
	}

	[DebuggerDisplay("{ToString()}")]
	[DebuggerNonUserCode]
	internal sealed class EventSetups
	{
		private Storage? _storage;

		public int Count
		{
			get
			{
				Storage? storage = _storage;
				if (storage is null)
				{
					return 0;
				}

				lock (storage)
				{
					return storage.List.Count;
				}
			}
		}

		public IEnumerable<(object? Target, MethodInfo Method)> Enumerate(string eventName)
		{
			Storage? storage = _storage;
			if (storage is null)
			{
				return [];
			}

			lock (storage)
			{
				List<(object? Target, MethodInfo Method, string Name)> list = storage.List;
				if (list.Count == 0)
				{
					return [];
				}

				List<(object?, MethodInfo)>? result = null;
				foreach ((object? target, MethodInfo method, string name) in list)
				{
					if (name == eventName)
					{
						result ??= [];
						result.Add((target, method));
					}
				}

				return result ?? [];
			}
		}

		public void Add(object? target, MethodInfo method, string eventName)
		{
			Storage storage = GetOrCreateStorage();
			lock (storage)
			{
				foreach ((object? Target, MethodInfo Method, string Name) entry in storage.List)
				{
					if (Matches(entry, target, method, eventName))
					{
						return;
					}
				}

				storage.List.Add((target, method, eventName));
			}
		}

		public void Remove(object? target, MethodInfo method, string eventName)
		{
			Storage? storage = _storage;
			if (storage is null)
			{
				return;
			}

			lock (storage)
			{
				List<(object? Target, MethodInfo Method, string Name)> list = storage.List;
				for (int i = 0; i < list.Count; i++)
				{
					if (Matches(list[i], target, method, eventName))
					{
						list.RemoveAt(i);
						return;
					}
				}
			}
		}

		private Storage GetOrCreateStorage()
		{
			if (_storage is null)
			{
				Interlocked.CompareExchange(ref _storage, new Storage(), null);
			}

			return _storage!;
		}

		private static bool Matches(
			(object? Target, MethodInfo Method, string Name) entry,
			object? target,
			MethodInfo method,
			string eventName)
			=> EqualityComparer<object?>.Default.Equals(entry.Target, target)
			   && entry.Method.Equals(method)
			   && entry.Name == eventName;

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			Storage? storage = _storage;
			if (storage is null)
			{
				return "0 events";
			}

			(object? Target, MethodInfo Method, string Name)[] snapshot;
			lock (storage)
			{
				List<(object? Target, MethodInfo Method, string Name)> list = storage.List;
				if (list.Count == 0)
				{
					return "0 events";
				}

				snapshot = list.ToArray();
			}

			StringBuilder sb = new();
			sb.Append(snapshot.Length).Append(snapshot.Length == 1 ? " event:" : " events:").AppendLine();
			foreach ((object? _, MethodInfo _, string name) in snapshot)
			{
				sb.Append(name).AppendLine();
			}

			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}

		private sealed class Storage
		{
			internal readonly List<(object? Target, MethodInfo Method, string Name)> List = [];
		}
	}
}
