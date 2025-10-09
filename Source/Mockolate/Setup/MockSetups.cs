using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Sets up the mock for <typeparamref name="T" />.
/// </summary>
[DebuggerDisplay("({_methodSetups}, {_propertySetups}, {_eventHandlers})")]
public class MockSetups<T>(IMock mock) : IMockSetup
{
	private readonly EventSetups _eventHandlers = new EventSetups();
	private readonly MethodSetups _methodSetups = new MethodSetups();
	private readonly PropertySetups _propertySetups = new PropertySetups();
	private readonly IndexerSetups _indexerSetups = new IndexerSetups();

	/// <summary>
	///     Retrieves the first method setup that matches the specified <paramref name="invocation" />,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	internal MethodSetup? GetMethodSetup(IInteraction invocation)
		=> _methodSetups.FirstOrDefault(setup => ((IMethodSetup)setup).Matches(invocation));

	/// <summary>
	///     Retrieves the setup configuration for the specified property name, creating a default setup if none exists.
	/// </summary>
	/// <remarks>
	///     If the specified property name does not have an associated setup, a default configuration is
	///     created and stored for future retrievals, so that getter and setter work in tandem.
	/// </remarks>
	internal PropertySetup GetPropertySetup(string propertyName)
	{
		if (!_propertySetups.TryGetValue(propertyName, out PropertySetup? matchingSetup))
		{
			if (mock.Behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException($"The property '{propertyName}' was accessed without prior setup.");
			}

			matchingSetup = new PropertySetup.Default();
			_propertySetups.TryAdd(propertyName, matchingSetup);
		}

		return matchingSetup;
	}

	/// <summary>
	///     Retrieves the setup configuration for the specified property name, creating a default setup if none exists.
	/// </summary>
	/// <remarks>
	///     If the specified property name does not have an associated setup, a default configuration is
	///     created and stored for future retrievals, so that getter and setter work in tandem.
	/// </remarks>
	internal PropertySetup GetIndexerSetup(object?[] parameters)
	{
		if (!_indexerSetups.TryGetValue(parameters, out PropertySetup? matchingSetup))
		{
			if (mock.Behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException($"The indexer [{string.Join(", ", parameters.Select(p => p?.ToString() ?? "null"))}] was accessed without prior setup.");
			}

			matchingSetup = new PropertySetup.Default();
			_indexerSetups.TryAdd(parameters, matchingSetup);
		}

		return matchingSetup;
	}

	/// <summary>
	///     A proxy implementation of <see cref="IMockSetup" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockSetup inner) : MockSetups<T>(inner.Mock), IMockSetup
	{
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
	}

	/// <summary>
	///     Sets up the protected elements of the mock for <typeparamref name="T" />.
	/// </summary>
	public class Protected(IMockSetup inner) : MockSetups<T>(inner.Mock), IMockSetup
	{
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
	}

	#region IMockSetup

	/// <inheritdoc cref="IMockSetup.Mock" />
	IMock IMockSetup.Mock => mock;

	/// <inheritdoc cref="IMockSetup.RegisterMethod(MethodSetup)" />
	void IMockSetup.RegisterMethod(MethodSetup methodSetup)
	{
		_methodSetups.Add(methodSetup);
	}

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
		foreach ((object? target, MethodInfo? method, string? name) in _eventHandlers)
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
	{
		_eventHandlers.Add(target, method, eventName);
	}

	/// <inheritdoc cref="IMockSetup.RemoveEvent(string, object?, MethodInfo)" />
	void IMockSetup.RemoveEvent(string eventName, object? target, MethodInfo method)
		=> _eventHandlers.Remove(target, method, eventName);

	#endregion IMockSetup

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
	{
		var sb = new StringBuilder();
		if (_methodSetups?.Count > 0)
		{
			sb.Append(_methodSetups.Count).Append(_methodSetups.Count == 1 ? " method, " : " methods, ");
		}

		if (_propertySetups?.Count > 0)
		{
			sb.Append(_propertySetups.Count).Append(_propertySetups.Count == 1 ? " property, " : " properties, ");
		}

		if (_eventHandlers?.Count > 0)
		{
			sb.Append(_eventHandlers.Count).Append(_eventHandlers.Count == 1 ? " event, " : " events, ");
		}

		if (_indexerSetups?.Count > 0)
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

	[DebuggerDisplay("{ToString()}")]
	private sealed class MethodSetups
	{
		private ConcurrentQueue<MethodSetup>? _storage;

		public void Add(MethodSetup setup)
		{
			_storage ??= new ConcurrentQueue<MethodSetup>();
			_storage.Enqueue(setup);
		}

		public MethodSetup? FirstOrDefault(Func<MethodSetup, bool> predicate)
		{
			if (_storage is null)
			{
				return null;
			}

			return _storage.FirstOrDefault(predicate);
		}

		public int Count
			=> _storage?.Count ?? 0;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
		{
			if (_storage is null || _storage.IsEmpty)
			{
				return "0 methods";
			}

			var sb = new StringBuilder();
			sb.Append(_storage.Count).Append(_storage.Count == 1 ? " method:" : " methods:").AppendLine();
			foreach (var methodSetup in _storage)
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

		public int Count
			=> _storage?.Count ?? 0;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
		{
			if (_storage is null || _storage.IsEmpty)
			{
				return "0 properties";
			}

			var sb = new StringBuilder();
			sb.Append(_storage.Count).Append(_storage.Count == 1 ? " property:" : " properties:").AppendLine();
			foreach (var item in _storage!)
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
		private Storage? _storage;

		public void TryAdd(object?[] parameters, PropertySetup setup)
		{
			_storage ??= new Storage();
			var storage = _storage;
			foreach (var parameter in parameters)
			{
				storage = storage.GetOrAdd(parameter, () => new Storage());
			}

			if (storage.Value is null)
			{
				Interlocked.Increment(ref _count);
				storage.Value = setup;
			}
		}

		public bool TryGetValue(object?[] parameters, [NotNullWhen(true)] out PropertySetup? setup)
		{
			_storage ??= new Storage();
			var storage = _storage;
			foreach (var parameter in parameters)
			{
				storage = storage.GetOrAdd(parameter, () => new Storage());
			}

			setup = storage.Value;
			return setup != null;
		}

		public int Count => _count;
		private int _count;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
		{
			if (_storage is null || Count == 0)
			{
				return "0 indexers";
			}

			var sb = new StringBuilder();
			sb.Append(Count).Append(Count == 1 ? " indexer:" : " indexers:").AppendLine();
			foreach (var item in _storage.GetSetups())
			{
				sb.Append(item.Path).Append(' ').Append(item.Setup).AppendLine();
			}
			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}
		private sealed class Storage
		{
			private ConcurrentDictionary<object, Storage> _storage = [];
			private Storage? _nullStorage;

			/// <summary>
			///     The value, if the storage is a leaf node.
			/// </summary>
			public PropertySetup? Value { get; set; }

			public IEnumerable<(string Path, PropertySetup Setup)> GetSetups()
			{
				foreach (var (path, setup) in GetSetups(""))
				{
					yield return ($"[{path}]", setup);
				}
			}

			private IEnumerable<(string, PropertySetup)> GetSetups(string prefix)
			{
				if (Value is not null)
				{
					yield return (prefix.TrimStart('.'), Value);
				}
				foreach (var item in _storage)
				{
					var itemPrefix = $"{prefix}.{item.Key?.ToString() ?? "null"}";
					foreach (var child in item.Value.GetSetups(itemPrefix))
					{
						yield return child;
					}
				}
			}

			public Storage GetOrAdd(object? key, Func<Storage> valueGenerator)
			{
				if (key is null)
				{
					if (_nullStorage is null)
					{
						_nullStorage = valueGenerator();
					}
					return _nullStorage;
				}
				return _storage.GetOrAdd(key, _ => valueGenerator());
			}
		}
	}

	[DebuggerDisplay("{ToString()}")]
	private sealed class EventSetups : IEnumerable<(object?, MethodInfo, string)>
	{
		private ConcurrentDictionary<(object?, MethodInfo, string), bool>? _storage;

		public IEnumerator<(object?, MethodInfo, string)> GetEnumerator()
		{
			if (_storage is null)
			{
				yield break;
			}
			foreach (var item in _storage.Keys)
			{
				yield return item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

		public int Count
			=> _storage?.Count ?? 0;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
		{
			if (_storage is null || _storage.IsEmpty)
			{
				return "0 events";
			}

			var sb = new StringBuilder();
			sb.Append(_storage.Count).Append(_storage.Count == 1 ? " event:" : " events:").AppendLine();
			foreach (var item in _storage.Keys)
			{
				sb.Append(item.Item3).AppendLine();
			}
			sb.Length -= Environment.NewLine.Length;
			return sb.ToString();
		}
	}
}
