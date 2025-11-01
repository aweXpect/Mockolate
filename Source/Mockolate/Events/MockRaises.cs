using System.Reflection;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate.Events;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Raise events on the mock for <typeparamref name="T" />.
/// </summary>
public class MockRaises<T>(IMockSetup setup, MockInteractions interactions) : IMockRaises
{
	/// <inheritdoc cref="IMockRaises.Setup" />
	IMockSetup IMockRaises.Setup
		=> setup;

	/// <inheritdoc cref="IMockRaises.Interactions" />
	MockInteractions IMockRaises.Interactions
		=> interactions;

	/// <inheritdoc cref="IMockRaises.Raise(string, object?[])" />
	void IMockRaises.Raise(string eventName, params object?[] parameters)
	{
		foreach ((object? target, MethodInfo? method) in setup.GetEventHandlers(eventName))
		{
			method.Invoke(target, parameters);
		}
	}

	/// <inheritdoc cref="IMockRaises.AddEvent(string, object?, MethodInfo?)" />
	void IMockRaises.AddEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event subscription may not be null.");
		}

		((IMockInteractions)interactions).RegisterInteraction(new EventSubscription(interactions.GetNextIndex(), name,
			target, method));
		setup.AddEvent(name, target, method);
	}

	/// <inheritdoc cref="IMockRaises.RemoveEvent(string, object?, MethodInfo?)" />
	void IMockRaises.RemoveEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event unsubscription may not be null.");
		}

		((IMockInteractions)interactions).RegisterInteraction(new EventUnsubscription(interactions.GetNextIndex(), name,
			target, method));
		setup.RemoveEvent(name, target, method);
	}
}
#pragma warning restore S2326 // Unused type parameters should be removed
