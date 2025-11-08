using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Mockolate.Exceptions;
using Mockolate.Interactions;

namespace Mockolate.Setup;

#pragma warning disable S1939 // Inheritance list should not be redundant
/// <summary>
///     Sets up the mock for <typeparamref name="T" />.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class MockSetup<T>(IMock mock, string prefix) : IMockSetup,
	IMockSetup<T>, IProtectedMockSetup<T>,
	IMockMethodSetup<T>, IProtectedMockMethodSetup<T>,
	IMockMethodSetupWithToString<T>, IMockMethodSetupWithEquals<T>, IMockMethodSetupWithGetHashCode<T>,
	IMockMethodSetupWithToStringWithEquals<T>, IMockMethodSetupWithToStringWithGetHashCode<T>,IMockMethodSetupWithEqualsWithGetHashCode<T>,
	IMockMethodSetupWithToStringWithEqualsWithGetHashCode<T>,
	IMockPropertySetup<T>, IProtectedMockPropertySetup<T>
{
	private readonly EventSetups _eventHandlers = new();
	private readonly IndexerSetups _indexerSetups = new();
	private readonly MethodSetups _methodSetups = new();
	private readonly PropertySetups _propertySetups = new();

	/// <summary>
	///     Retrieves the latest method setup that matches the specified <paramref name="methodInvocation" />,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	internal MethodSetup? GetMethodSetup(MethodInvocation methodInvocation)
		=> _methodSetups.GetLatestOrDefault(setup => ((IMethodSetup)setup).Matches(methodInvocation));

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
	internal PropertySetup GetPropertySetup(string propertyName, Func<object?>? defaultValueGenerator)
	{
		if (!_propertySetups.TryGetValue(propertyName, out PropertySetup? matchingSetup))
		{
			if (mock.Behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException($"The property '{propertyName}' was accessed without prior setup.");
			}

			matchingSetup = new PropertySetup.Default(defaultValueGenerator?.Invoke());
			_propertySetups.TryAdd(propertyName, matchingSetup);
		}

		return matchingSetup;
	}

	/// <summary>
	///     Retrieves the latest indexer setup that matches the specified <paramref name="interaction" />,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	internal IndexerSetup? GetIndexerSetup(IndexerAccess interaction)
		=> _indexerSetups.GetLastestOrDefault(setup => ((IIndexerSetup)setup).Matches(interaction));

	/// <summary>
	///     Gets the indexer value for the given <paramref name="parameters" />.
	/// </summary>
	internal TValue GetIndexerValue<TValue>(IIndexerSetup? setup, Func<TValue>? defaultValueGenerator,
		object?[] parameters)
		=> _indexerSetups.GetOrAddValue(parameters, () =>
		{
			if (setup?.TryGetInitialValue(mock.Behavior, parameters, out TValue? value) == true)
			{
				return value;
			}

			if (defaultValueGenerator is not null)
			{
				return defaultValueGenerator();
			}

			if (mock.Behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException(
					$"The indexer [{string.Join(", ", parameters.Select(p => p?.ToString() ?? "null"))}] was accessed without prior setup.");
			}

			return mock.Behavior.DefaultValue.Generate<TValue>();
		});

	/// <inheritdoc cref="object.ToString()" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		StringBuilder? sb = new();
		if (_methodSetups.Count > 0)
		{
			sb.Append(_methodSetups.Count).Append(_methodSetups.Count == 1 ? " method, " : " methods, ");
		}

		if (_propertySetups.Count > 0)
		{
			sb.Append(_propertySetups.Count).Append(_propertySetups.Count == 1 ? " property, " : " properties, ");
		}

		if (_eventHandlers.Count > 0)
		{
			sb.Append(_eventHandlers.Count).Append(_eventHandlers.Count == 1 ? " event, " : " events, ");
		}

		if (_indexerSetups.Count > 0)
		{
			sb.Append(_indexerSetups.Count).Append(_indexerSetups.Count == 1 ? " indexer, " : " indexers, ");
		}

		if (sb.Length < 2)
		{
			return "(none)";
		}

		sb.Length -= 2;
		return sb.ToString();
	}

	/// <summary>
	///     A proxy implementation of <see cref="IMockSetup" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockSetup inner, string prefix) : MockSetup<T>(inner.Mock, prefix), IMockSetup
	{
		/// <inheritdoc cref="IMockSetup.Mock" />
		public IMock Mock
			=> inner.Mock;

		/// <inheritdoc cref="IMockSetup.RegisterIndexer(IndexerSetup)" />
		void IMockSetup.RegisterIndexer(IndexerSetup indexerSetup)
			=> inner.RegisterIndexer(indexerSetup);

		/// <inheritdoc cref="IMockSetup.RegisterMethod(MethodSetup)" />
		void IMockSetup.RegisterMethod(MethodSetup methodSetup)
			=> inner.RegisterMethod(methodSetup);

		/// <inheritdoc cref="IMockSetup.RegisterProperty(string, PropertySetup)" />
		void IMockSetup.RegisterProperty(string propertyName, PropertySetup propertySetup)
			=> inner.RegisterProperty(propertyName, propertySetup);

		/// <inheritdoc cref="IMockSetup.GetEventHandlers(string)" />
		IEnumerable<(object?, MethodInfo)> IMockSetup.GetEventHandlers(string eventName)
			=> inner.GetEventHandlers(eventName);

		/// <inheritdoc cref="IMockSetup.AddEvent(string, object?, MethodInfo)" />
		void IMockSetup.AddEvent(string eventName, object? target, MethodInfo method)
			=> inner.AddEvent(eventName, target, method);

		/// <inheritdoc cref="IMockSetup.RemoveEvent(string, object?, MethodInfo)" />
		void IMockSetup.RemoveEvent(string eventName, object? target, MethodInfo method)
			=> inner.RemoveEvent(eventName, target, method);

		/// <inheritdoc cref="IMockSetup.SetIndexerValue{TValue}(object?[], TValue)" />
		public void SetIndexerValue<TValue>(object?[] parameters, TValue value)
			=> inner.SetIndexerValue(parameters, value);
	}

	[DebuggerDisplay("{ToString()}")]
	private sealed class MethodSetups
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

		/// <inheritdoc cref="object.ToString()" />
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			if (_storage is null || _storage.IsEmpty)
			{
				return "0 methods";
			}

			StringBuilder? sb = new();
			sb.Append(_storage.Count).Append(_storage.Count == 1 ? " method:" : " methods:").AppendLine();
			foreach (MethodSetup? methodSetup in _storage)
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
		private ConcurrentDictionary<string, PropertySetup>? _storage;

		public int Count
			=> _storage?.Count ?? 0;

		public bool TryAdd(string propertyName, PropertySetup setup)
		{
			_storage ??= new ConcurrentDictionary<string, PropertySetup>();
			return _storage.TryAdd(propertyName, setup);
		}

		public bool TryGetValue(string propertyName, [NotNullWhen(true)] out PropertySetup? setup)
		{
			_storage ??= new ConcurrentDictionary<string, PropertySetup>();
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

			StringBuilder? sb = new();
			sb.Append(_storage.Count).Append(_storage.Count == 1 ? " property:" : " properties:").AppendLine();
			foreach (KeyValuePair<string, PropertySetup> item in _storage!)
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

			StringBuilder? sb = new();
			sb.Append(Count).Append(Count == 1 ? " indexer:" : " indexers:").AppendLine();
			foreach (IndexerSetup? indexerSetup in _storage)
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
			private object? _value;
			public bool HasValue { get; private set; }

			public object? Value
			{
				get => _value;
				set
				{
					_value = value;
					HasValue = true;
				}
			}

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

			StringBuilder? sb = new();
			sb.Append(_storage.Count).Append(_storage.Count == 1 ? " event:" : " events:").AppendLine();
			foreach ((object?, MethodInfo, string) item in _storage.Keys)
			{
				sb.Append(item.Item3).AppendLine();
			}

			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}
	}

	#region IMockSetup

	/// <inheritdoc cref="IMockSetup.Mock" />
	IMock IMockSetup.Mock => mock;

	/// <inheritdoc cref="IMockSetup.RegisterIndexer(IndexerSetup)" />
	void IMockSetup.RegisterIndexer(IndexerSetup indexerSetup) => _indexerSetups.Add(indexerSetup);

	/// <inheritdoc cref="IMockSetup.SetIndexerValue{TValue}(object?[], TValue)" />
	void IMockSetup.SetIndexerValue<TValue>(object?[] parameters, TValue value)
		=> _indexerSetups.UpdateValue(parameters, value);

	/// <inheritdoc cref="IMockSetup.RegisterMethod(MethodSetup)" />
	void IMockSetup.RegisterMethod(MethodSetup methodSetup) => _methodSetups.Add(methodSetup);

	/// <inheritdoc cref="IMockSetup.RegisterProperty(string, PropertySetup)" />
	void IMockSetup.RegisterProperty(string propertyName, PropertySetup propertySetup)
	{
		if (!_propertySetups.TryAdd(propertyName, propertySetup))
		{
			throw new MockException($"You cannot setup property '{propertyName}' twice.");
		}
	}

	/// <inheritdoc cref="IMockSetup.GetEventHandlers(string)" />
	IEnumerable<(object?, MethodInfo)> IMockSetup.GetEventHandlers(string eventName)
	{
		foreach ((object? target, MethodInfo? method, string? name) in _eventHandlers.Enumerate())
		{
			if (name != eventName)
			{
				continue;
			}

			yield return (target, method);
		}
	}

	/// <inheritdoc cref="IMockSetup.AddEvent(string, object?, MethodInfo)" />
	void IMockSetup.AddEvent(string eventName, object? target, MethodInfo method)
		=> _eventHandlers.Add(target, method, eventName);

	/// <inheritdoc cref="IMockSetup.RemoveEvent(string, object?, MethodInfo)" />
	void IMockSetup.RemoveEvent(string eventName, object? target, MethodInfo method)
		=> _eventHandlers.Remove(target, method, eventName);

	/// <inheritdoc cref="IMockMethodSetupWithToString{T}.ToString()" />
	ReturnMethodSetup<string> IMockMethodSetupWithToString<T>.ToString()
	{
		var methodSetup = new ReturnMethodSetup<string>(prefix + ".ToString");
		_methodSetups.Add(methodSetup);
		return methodSetup;
	}

	ReturnMethodSetup<bool, object?> IMockMethodSetupWithEquals<T>.Equals(Match.IParameter<object?> obj)
	{
		var methodSetup = new ReturnMethodSetup<bool, object?>(prefix + ".Equals", new Match.NamedParameter("obj", obj));
		_methodSetups.Add(methodSetup);
		return methodSetup;
	}
	ReturnMethodSetup<int> IMockMethodSetupWithGetHashCode<T>.GetHashCode()
	{
		var methodSetup = new ReturnMethodSetup<int>(prefix + ".GetHashCode");
		_methodSetups.Add(methodSetup);
		return methodSetup;
	}

	#endregion IMockSetup
}
#pragma warning restore S1939 // Inheritance list should not be redundant
