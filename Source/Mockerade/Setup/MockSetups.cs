using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mockerade.Checks;

namespace Mockerade.Setup;

/// <summary>
///     Sets up the <see cref="Mock{T}" /> <paramref name="mock" />
/// </summary>
public class MockSetups<T>(IMock mock) : IMockSetup
{
	/// <summary>
	/// A proxy implementation of <see cref="IMockSetup"/> that forwards all calls to the provided <paramref name="inner"/> instance.
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

	private readonly Dictionary<string, PropertySetup> _propertySetups = [];
	private readonly List<MethodSetup> _methodSetups = [];
	private ConcurrentDictionary<(object?, MethodInfo, string), bool>? _eventHandlers;

	#region IMockSetup

	/// <inheritdoc cref="IMockSetup.Mock" />
	IMock IMockSetup.Mock => mock;

	/// <inheritdoc cref="IMockSetup.RegisterMethod(MethodSetup)" />
	void IMockSetup.RegisterMethod(MethodSetup methodSetup)
	{
		if (mock.Invocations.IsAlreadyInvoked)
		{
			throw new NotSupportedException("You may not register additional setups after the first usage of the mock");
		}

		_methodSetups.Add(methodSetup);
	}

	/// <inheritdoc cref="IMockSetup.RegisterProperty(string, PropertySetup)" />
	void IMockSetup.RegisterProperty(string propertyName, PropertySetup propertySetup)
	{
		if (mock.Invocations.IsAlreadyInvoked)
		{
			throw new NotSupportedException("You may not register additional setups after the first usage of the mock");
		}

		_propertySetups.Add(propertyName, propertySetup);
	}

	/// <inheritdoc cref="IMockSetup.GetEventHandlers(string)" />
	IEnumerable<(object?, MethodInfo)> IMockSetup.GetEventHandlers(string eventName)
	{
		if (_eventHandlers is null)
		{
			yield break;
		}

		foreach (var (target, method, name) in _eventHandlers.Keys)
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
		_eventHandlers ??= new ConcurrentDictionary<(object?, MethodInfo, string), bool>();
		_eventHandlers.TryAdd((target, method, eventName), true);
	}

	/// <inheritdoc cref="IMockSetup.RemoveEvent(string, object?, MethodInfo)" />
	void IMockSetup.RemoveEvent(string eventName, object? target, MethodInfo method)
	{
		_eventHandlers?.TryRemove((target, method, eventName), out _);
	}

	#endregion IMockSetup

	/// <summary>
	///     Retrieves the first method setup that matches the specified <paramref name="invocation" />,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	internal MethodSetup? GetMethodSetup(Invocation invocation)
	{
		return _methodSetups.FirstOrDefault(setup => ((IMethodSetup)setup).Matches(invocation));
	}

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
			matchingSetup = new PropertySetup.Default();
			_propertySetups.Add(propertyName, matchingSetup);
		}

		return matchingSetup;
	}
}
