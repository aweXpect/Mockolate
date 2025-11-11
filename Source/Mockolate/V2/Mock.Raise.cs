using System.Collections.Generic;
using System.Reflection;
using Mockolate.Events;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate.V2;

public partial class Mock<T>
{
	/// <inheritdoc cref="IMockRaises.Raise" />
	public void Raise(string eventName, params object?[] parameters)
	{
		foreach ((object? target, MethodInfo? method) in GetEventHandlers(eventName))
		{
			method.Invoke(target, parameters);
		}
	}

	/// <inheritdoc cref="IMockRaises.AddEvent(string, object?, MethodInfo?)" />
	public void AddEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event subscription may not be null.");
		}

		((IMockInteractions)Interactions).RegisterInteraction(new EventSubscription(Interactions.GetNextIndex(), name,
			target, method));
		AddEvent(name, target, method);
	}

	/// <inheritdoc cref="IMockRaises.RemoveEvent(string, object?, MethodInfo?)" />
	public void RemoveEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event unsubscription may not be null.");
		}

		((IMockInteractions)Interactions).RegisterInteraction(new EventUnsubscription(Interactions.GetNextIndex(), name,
			target, method));
		RemoveEvent(name, target, method);
	}


	/// <inheritdoc cref="IMockSetup.GetEventHandlers(string)" />
	public IEnumerable<(object?, MethodInfo)> GetEventHandlers(string eventName)
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
}
