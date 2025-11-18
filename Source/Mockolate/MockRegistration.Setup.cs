using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate;

public partial class MockRegistration
{
	private readonly EventSetups _eventHandlers = new();
	private readonly IndexerSetups _indexerSetups = new();
	private readonly MethodSetups _methodSetups = new();
	private readonly PropertySetups _propertySetups = new();

	/// <summary>
	///     Retrieves the latest method setup that matches the specified <paramref name="methodInvocation" />,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private IMethodSetup? GetMethodSetup(MethodInvocation methodInvocation)
		=> _methodSetups.GetLatestOrDefault(setup => setup.Matches(methodInvocation));

	/// <summary>
	///     Retrieves the setup configuration for the specified property name, creating a default setup if none exists.
	/// </summary>
	/// <remarks>
	///     If the specified property name does not have an associated setup, and the mock is configured to throw when not set
	///     up,
	///     a <see cref="MockNotSetupException" /> is thrown. Otherwise, a default value is created and stored for future
	///     retrievals,
	///     so that getter and setter work in tandem.
	/// </remarks>
	private IPropertySetup GetPropertySetup(string propertyName, Func<object?>? defaultValueGenerator)
	{
		if (!_propertySetups.TryGetValue(propertyName, out IPropertySetup? matchingSetup))
		{
			if (Behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException($"The property '{propertyName}' was accessed without prior setup.");
			}

			if (!Behavior.CallBaseClass)
			{
				defaultValueGenerator = null;
			}

			matchingSetup = new PropertySetup.Default(defaultValueGenerator?.Invoke());
			_propertySetups.TryAdd(propertyName, matchingSetup);
		}
		else if (defaultValueGenerator is not null && (matchingSetup.CallBaseClass() ?? Behavior.CallBaseClass))
		{
			matchingSetup.InitializeWith(defaultValueGenerator());
		}

		return matchingSetup;
	}

	/// <summary>
	///     Retrieves the latest indexer setup that matches the specified <paramref name="interaction" />,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private IndexerSetup? GetIndexerSetup(IndexerAccess interaction)
		=> _indexerSetups.GetLastestOrDefault(setup => ((IIndexerSetup)setup).Matches(interaction));

	/// <summary>
	///     Gets the indexer value for the given <paramref name="parameters" />.
	/// </summary>
	private TValue GetIndexerValue<TValue>(IIndexerSetup? setup, Func<TValue> defaultValueGenerator,
		object?[] parameters)
		=> _indexerSetups.GetOrAddValue(parameters, defaultValueGenerator);

	/// <summary>
	///     Registers the <paramref name="indexerSetup" /> in the mock.
	/// </summary>
	public void SetupIndexer(IndexerSetup indexerSetup) => _indexerSetups.Add(indexerSetup);

	/// <summary>
	///     Registers the <paramref name="methodSetup" /> in the mock.
	/// </summary>
	public void SetupMethod(IMethodSetup methodSetup) => _methodSetups.Add(methodSetup);

	/// <summary>
	///     Registers the <paramref name="propertySetup" /> in the mock.
	/// </summary>
	public void SetupProperty(string propertyName, IPropertySetup propertySetup)
	{
		if (!_propertySetups.TryAdd(propertyName, propertySetup))
		{
			throw new MockException($"You cannot setup property '{propertyName}' twice.");
		}
	}

	[DebuggerDisplay("{ToString()}")]
	private sealed class MethodSetups
	{
		private ConcurrentStack<IMethodSetup>? _storage;

		public int Count
			=> _storage?.Count ?? 0;

		public void Add(IMethodSetup setup)
		{
			_storage ??= new ConcurrentStack<IMethodSetup>();
			_storage.Push(setup);
		}

		public IMethodSetup? GetLatestOrDefault(Func<IMethodSetup, bool> predicate)
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
			if (_storage is null || _storage.IsEmpty)
			{
				return "0 methods";
			}

			StringBuilder sb = new();
			sb.Append(_storage.Count).Append(_storage.Count == 1 ? " method:" : " methods:").AppendLine();
			foreach (IMethodSetup methodSetup in _storage)
			{
				sb.Append(methodSetup).AppendLine();
			}

			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}
	}

	[DebuggerDisplay("{ToString()}")]
	private sealed class PropertySetups
	{
		private ConcurrentDictionary<string, IPropertySetup>? _storage;

		public int Count
			=> _storage?.Count ?? 0;

		public bool TryAdd(string propertyName, IPropertySetup setup)
		{
			_storage ??= new ConcurrentDictionary<string, IPropertySetup>();
			return _storage.TryAdd(propertyName, setup);
		}

		public bool TryGetValue(string propertyName, [NotNullWhen(true)] out IPropertySetup? setup)
		{
			_storage ??= new ConcurrentDictionary<string, IPropertySetup>();
			return _storage.TryGetValue(propertyName, out setup);
		}

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			if (_storage is null || _storage.IsEmpty)
			{
				return "0 properties";
			}

			StringBuilder sb = new();
			sb.Append(_storage.Count).Append(_storage.Count == 1 ? " property:" : " properties:").AppendLine();
			foreach (KeyValuePair<string, IPropertySetup> item in _storage!)
			{
				sb.Append(item.Value).Append(' ').Append(item.Key).AppendLine();
			}

			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}
	}

	[DebuggerDisplay("{ToString()}")]
	private sealed class IndexerSetups
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

		internal TValue GetOrAddValue<TValue>(object?[] parameters, Func<TValue> valueGenerator)
		{
			_valueStorage ??= new ValueStorage();
			ValueStorage? storage = _valueStorage;
			foreach (object? parameter in parameters)
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

		internal void UpdateValue<TValue>(object?[] parameters, TValue value)
		{
			_valueStorage ??= new ValueStorage();
			ValueStorage? storage = _valueStorage;
			foreach (object? parameter in parameters)
			{
				storage = storage.GetOrAdd(parameter, () => new ValueStorage());
			}

			storage.Value = value;
		}

		private sealed class ValueStorage
		{
			private ValueStorage? _nullStorage;
			private ConcurrentDictionary<object, ValueStorage>? _storage = [];

			public object? Value { get; set; }

			public ValueStorage GetOrAdd(object? key, Func<ValueStorage> valueGenerator)
			{
				if (key is null)
				{
					if (_nullStorage is null)
					{
						_nullStorage = valueGenerator();
					}

					return _nullStorage;
				}

				_storage ??= new ConcurrentDictionary<object, ValueStorage>();
				return _storage.GetOrAdd(key, _ => valueGenerator());
			}
		}
	}

	[DebuggerDisplay("{ToString()}")]
	private sealed class EventSetups
	{
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
			_storage.TryAdd((target, method, eventName), true);
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
