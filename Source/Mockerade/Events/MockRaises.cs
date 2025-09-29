using System;
using System.Reflection;
using Mockerade.Checks;
using Mockerade.Setup;

namespace Mockerade.Events;

/// <summary>
///     Allows raising events on the mock.
/// </summary>
public class MockRaises<T>(IMockSetup setup, Checks.MockInvocations invocations) : IMockRaises
{
	/// <inheritdoc cref="IMockRaises.Raise(string, object?[])" />
	void IMockRaises.Raise(string eventName, params object?[] parameters)
	{
		foreach (var(target, method) in setup.GetEventHandlers(eventName))
		{
			method.Invoke(target, parameters);
		}
	}

	/// <inheritdoc cref="IMockRaises.AddEvent(string, object?, MethodInfo?)" />
	void IMockRaises.AddEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			// TODO: Throw exception?
			return;
		}

		invocations.RegisterInvocation(new EventSubscription(name, target, method));
		setup.AddEvent(name, target, method);
	}

	/// <inheritdoc cref="IMockRaises.RemoveEvent(string, object?, MethodInfo?)" />
	void IMockRaises.RemoveEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			// TODO: Throw exception?
			return;
		}

		invocations.RegisterInvocation(new EventUnsubscription(name, target, method));
		setup.RemoveEvent(name, target, method);
	}
}
