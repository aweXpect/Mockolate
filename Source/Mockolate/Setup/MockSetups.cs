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
		private readonly List<MethodSetup> _list = new();
		private readonly object _lock = new();

		public int Count
		{
			get { lock (_lock) { return _list.Count; } }
		}

		public void Add(MethodSetup setup)
		{
			lock (_lock)
			{
				_list.Add(setup);
			}
		}

		public MethodSetup? GetLatestOrDefault(Func<MethodSetup, bool> predicate)
		{
			MethodSetup[] snapshot;
			lock (_lock)
			{
				if (_list.Count == 0)
				{
					return null;
				}

				snapshot = _list.ToArray();
			}

			for (int i = snapshot.Length - 1; i >= 0; i--)
			{
				if (predicate(snapshot[i]))
				{
					return snapshot[i];
				}
			}

			return null;
		}

		internal IEnumerable<MethodSetup> EnumerateUnusedSetupsBy(MockInteractions interactions)
		{
			MethodSetup[] snapshot;
			lock (_lock)
			{
				if (_list.Count == 0)
				{
					return [];
				}

				snapshot = _list.ToArray();
			}

			return snapshot.Where(methodSetup => interactions.Interactions.OfType<MethodInvocation>()
				.All(methodInvocation => !((IInteractiveMethodSetup)methodSetup).Matches(methodInvocation)));
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			MethodSetup[] snapshot;
			lock (_lock)
			{
				if (_list.Count == 0)
				{
					return "0 methods";
				}

				snapshot = _list.ToArray();
			}

			StringBuilder sb = new();
			sb.Append(snapshot.Length).Append(snapshot.Length == 1 ? " method:" : " methods:").AppendLine();
			foreach (MethodSetup methodSetup in snapshot)
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
		private volatile PropertySetup[] _storage = [];
		private readonly object _writeLock = new();
		private volatile int _count;

		public int Count => _count;

		public void Add(PropertySetup setup)
		{
			lock (_writeLock)
			{
				PropertySetup[] current = _storage;
				for (int i = 0; i < current.Length; i++)
				{
					if (current[i].Name != setup.Name)
					{
						continue;
					}

					PropertySetup[] updated = new PropertySetup[current.Length];
					current.CopyTo(updated, 0);
					bool wasDefault = current[i] is PropertySetup.Default;
					bool isDefault = setup is PropertySetup.Default;
					updated[i] = setup;
					_storage = updated;
					if (wasDefault && !isDefault)
					{
						_count++;
					}
					else if (!wasDefault && isDefault)
					{
						_count--;
					}

					return;
				}

				PropertySetup[] next = new PropertySetup[current.Length + 1];
				current.CopyTo(next, 0);
				next[current.Length] = setup;
				_storage = next;
				if (setup is not PropertySetup.Default)
				{
					_count++;
				}
			}
		}

		public bool TryGetValue(string propertyName, [NotNullWhen(true)] out PropertySetup? setup)
		{
			PropertySetup[] snapshot = _storage;
			for (int i = 0; i < snapshot.Length; i++)
			{
				if (snapshot[i].Name == propertyName)
				{
					setup = snapshot[i];
					return true;
				}
			}

			setup = null;
			return false;
		}

		internal IEnumerable<PropertySetup> EnumerateUnusedSetupsBy(MockInteractions interactions)
		{
			PropertySetup[] snapshot = _storage;
			if (snapshot.Length == 0)
			{
				return [];
			}

			return snapshot.Where(propertySetup => interactions.Interactions.OfType<PropertyAccess>()
				.All(propertyAccess => !((IInteractivePropertySetup)propertySetup).Matches(propertyAccess)));
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			PropertySetup[] snapshot;
			lock (_writeLock)
			{
				snapshot = _storage;
			}

			int count = 0;
			foreach (PropertySetup setup in snapshot)
			{
				if (setup is not PropertySetup.Default)
				{
					count++;
				}
			}

			if (count == 0)
			{
				return "0 properties";
			}

			StringBuilder sb = new();
			sb.Append(count).Append(count == 1 ? " property:" : " properties:").AppendLine();
			foreach (PropertySetup setup in snapshot)
			{
				if (setup is not PropertySetup.Default)
				{
					sb.Append(setup).Append(' ').Append(setup.Name).AppendLine();
				}
			}

			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}
	}

	[DebuggerDisplay("{ToString()}")]
	[DebuggerNonUserCode]
	internal sealed class IndexerSetups
	{
		private readonly List<IndexerSetup> _list = new();
		private readonly object _lock = new();

		private ValueStorage? _valueStorage;

		public int Count
		{
			get { lock (_lock) { return _list.Count; } }
		}

		public IndexerSetup? GetLatestOrDefault(Func<IndexerSetup, bool> predicate)
		{
			IndexerSetup[] snapshot;
			lock (_lock)
			{
				if (_list.Count == 0)
				{
					return null;
				}

				snapshot = _list.ToArray();
			}

			for (int i = snapshot.Length - 1; i >= 0; i--)
			{
				if (predicate(snapshot[i]))
				{
					return snapshot[i];
				}
			}

			return null;
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			IndexerSetup[] snapshot;
			lock (_lock)
			{
				if (_list.Count == 0)
				{
					return "0 indexers";
				}

				snapshot = _list.ToArray();
			}

			StringBuilder sb = new();
			sb.Append(snapshot.Length).Append(snapshot.Length == 1 ? " indexer:" : " indexers:").AppendLine();
			foreach (IndexerSetup indexerSetup in snapshot)
			{
				sb.Append(indexerSetup).AppendLine();
			}

			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}

		internal void Add(IndexerSetup setup)
		{
			lock (_lock)
			{
				_list.Add(setup);
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
			IndexerSetup[] snapshot;
			lock (_lock)
			{
				if (_list.Count == 0)
				{
					return [];
				}

				snapshot = _list.ToArray();
			}

			return snapshot.Where(indexerSetup => interactions.Interactions.OfType<IndexerAccess>()
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
		private readonly List<(object? Target, MethodInfo Method, string Name)> _list = new();
		private readonly object _lock = new();

		public int Count
		{
			get { lock (_lock) { return _list.Count; } }
		}

		public (object? Target, MethodInfo Method, string Name)[] Enumerate()
		{
			lock (_lock)
			{
				return _list.ToArray();
			}
		}

		public void Add(object? target, MethodInfo method, string eventName)
		{
			lock (_lock)
			{
				foreach ((object? Target, MethodInfo Method, string Name) entry in _list)
				{
					if (Matches(entry, target, method, eventName))
					{
						return;
					}
				}

				_list.Add((target, method, eventName));
			}
		}

		public void Remove(object? target, MethodInfo method, string eventName)
		{
			lock (_lock)
			{
				for (int i = 0; i < _list.Count; i++)
				{
					if (Matches(_list[i], target, method, eventName))
					{
						_list.RemoveAt(i);
						return;
					}
				}
			}
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
			(object? Target, MethodInfo Method, string Name)[] snapshot;
			lock (_lock)
			{
				if (_list.Count == 0)
				{
					return "0 events";
				}

				snapshot = _list.ToArray();
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
	}
}
