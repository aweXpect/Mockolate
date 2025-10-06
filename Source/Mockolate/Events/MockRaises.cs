using System.Reflection;
using Mockolate.Checks;
using Mockolate.Checks.Interactions;
using Mockolate.Exceptions;
using Mockolate.Setup;

namespace Mockolate.Events;

/// <summary>
///     Raise events on the mock for <typeparamref name="T" />.
/// </summary>
public class MockRaises<T>(IMockSetup setup, Checks.Checks checks) : IMockRaises
{
	/// <inheritdoc cref="IMockRaises.Raise(string, object?[])" />
	void IMockRaises.Raise(string eventName, params object?[] parameters)
	{
		foreach (var (target, method) in setup.GetEventHandlers(eventName))
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

		checks.RegisterInvocation(new EventSubscription(checks.GetNextIndex(), name, target, method));
		setup.AddEvent(name, target, method);
	}

	/// <inheritdoc cref="IMockRaises.RemoveEvent(string, object?, MethodInfo?)" />
	void IMockRaises.RemoveEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event unsubscription may not be null.");
		}

		checks.RegisterInvocation(new EventUnsubscription(checks.GetNextIndex(), name, target, method));
		setup.RemoveEvent(name, target, method);
	}

	/// <summary>
	///     Raise protected events on the mock for <typeparamref name="T" />.
	/// </summary>
	public class Protected(IMockRaises inner, IMockSetup setup, Checks.Checks checks)
		: MockRaises<T>(setup, checks), IMockRaises
	{
		/// <inheritdoc cref="IMockRaises.Raise(string, object?[])" />
		void IMockRaises.Raise(string eventName, params object?[] parameters)
			=> inner.Raise(eventName, parameters);

		/// <inheritdoc cref="IMockRaises.AddEvent(string, object?, MethodInfo?)" />
		void IMockRaises.AddEvent(string name, object? target, MethodInfo? method)
			=> inner.AddEvent(name, target, method);

		/// <inheritdoc cref="IMockRaises.RemoveEvent(string, object?, MethodInfo?)" />
		void IMockRaises.RemoveEvent(string name, object? target, MethodInfo? method)
			=> inner.RemoveEvent(name, target, method);
	}
}
