using System;
using System.Reflection;

namespace Mockolate.Setup;

/// <summary>
///     Interface for setting up an event with fluent syntax.
/// </summary>
public interface IEventSetup
{
	/// <summary>
	///     Sets up callbacks on the event subscription (add accessor).
	/// </summary>
	IEventSubscriptionSetup OnSubscribed { get; }

	/// <summary>
	///     Sets up callbacks on the event unsubscription (remove accessor).
	/// </summary>
	IEventUnsubscriptionSetup OnUnsubscribed { get; }
}

/// <summary>
///     Interface for setting up an event subscription with fluent syntax.
/// </summary>
public interface IEventSubscriptionSetup
{
	/// <summary>
	///     Registers a callback to be invoked whenever a handler is subscribed to the event.
	/// </summary>
	IEventSetupCallbackBuilder Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever a handler is subscribed to the event.
	/// </summary>
	/// <remarks>
	///     The callback receives the target object and method of the subscribed handler.
	/// </remarks>
	IEventSetupCallbackBuilder Do(Action<object?, MethodInfo> callback);
}

/// <summary>
///     Interface for setting up an event unsubscription with fluent syntax.
/// </summary>
public interface IEventUnsubscriptionSetup
{
	/// <summary>
	///     Registers a callback to be invoked whenever a handler is unsubscribed from the event.
	/// </summary>
	IEventSetupCallbackBuilder Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever a handler is unsubscribed from the event.
	/// </summary>
	/// <remarks>
	///     The callback receives the target object and method of the unsubscribed handler.
	/// </remarks>
	IEventSetupCallbackBuilder Do(Action<object?, MethodInfo> callback);
}

/// <summary>
///     Interface for setting up an event with fluent syntax.
/// </summary>
public interface IEventSetupCallbackBuilder : IEventSetupCallbackWhenBuilder
{
	/// <summary>
	///     Limits the callback to only execute for event interactions where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the event has been interacted with so far.
	/// </remarks>
	IEventSetupCallbackWhenBuilder When(Func<int, bool> predicate);

	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IEventSetupCallbackWhenBuilder InParallel();
}

/// <summary>
///     Interface for setting up an event with fluent syntax.
/// </summary>
public interface IEventSetupCallbackWhenBuilder : IEventSetup
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IEventSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IEventSetupCallbackWhenBuilder For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IEventSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IEventSetup Only(int times);
}
