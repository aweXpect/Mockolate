using System.Reflection;
using Mockolate.Checks;
using Mockolate.Setup;

namespace Mockolate.Events;

/// <summary>
///     Raise events on the mock for <typeparamref name="T"/>.
/// </summary>
public class MockRaises<T>(IMockSetup setup, MockInvocations invocations) : IMockRaises
{
	/// <summary>
	///     Raise protected events on the mock for <typeparamref name="T"/>.
	/// </summary>
	public class Protected(IMockRaises inner, IMockSetup setup, MockInvocations invocations) : MockRaises<T>(setup, invocations), IMockRaises
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
