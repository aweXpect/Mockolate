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
	IEventSubscriptionSetupCallbackBuilder Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever a handler is subscribed to the event.
	/// </summary>
	/// <remarks>
	///     The callback receives the target object and method of the subscribed handler.
	/// </remarks>
	IEventSubscriptionSetupCallbackBuilder Do(Action<object?, MethodInfo> callback);

	/// <summary>
	///     Transitions the scenario to the given <paramref name="scenario" /> whenever a handler is subscribed to the event.
	/// </summary>
	/// <param name="scenario">The name of the new scenario.</param>
	IEventSubscriptionSetupParallelCallbackBuilder TransitionTo(string scenario);
}

/// <summary>
///     Interface for setting up an event unsubscription with fluent syntax.
/// </summary>
public interface IEventUnsubscriptionSetup
{
	/// <summary>
	///     Registers a callback to be invoked whenever a handler is unsubscribed from the event.
	/// </summary>
	IEventUnsubscriptionSetupCallbackBuilder Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever a handler is unsubscribed from the event.
	/// </summary>
	/// <remarks>
	///     The callback receives the target object and method of the unsubscribed handler.
	/// </remarks>
	IEventUnsubscriptionSetupCallbackBuilder Do(Action<object?, MethodInfo> callback);

	/// <summary>
	///     Transitions the scenario to the given <paramref name="scenario" /> whenever a handler is unsubscribed from the
	///     event.
	/// </summary>
	/// <param name="scenario">The name of the new scenario.</param>
	IEventUnsubscriptionSetupParallelCallbackBuilder TransitionTo(string scenario);
}

/// <summary>
///     Interface for setting up an event subscription with fluent syntax.
/// </summary>
public interface IEventSubscriptionSetupParallelCallbackBuilder : IEventSubscriptionSetupCallbackWhenBuilder
{
	/// <summary>
	///     Limits the callback to only execute for event interactions where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the event has been interacted with so far.
	/// </remarks>
	IEventSubscriptionSetupCallbackWhenBuilder When(Func<int, bool> predicate);
}

/// <summary>
///     Interface for setting up an event subscription with fluent syntax.
/// </summary>
public interface IEventSubscriptionSetupCallbackBuilder : IEventSubscriptionSetupParallelCallbackBuilder
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IEventSubscriptionSetupParallelCallbackBuilder InParallel();
}

/// <summary>
///     Interface for setting up an event subscription with fluent syntax.
/// </summary>
public interface IEventSubscriptionSetupCallbackWhenBuilder : IEventSetup
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IEventSubscriptionSetupParallelCallbackBuilder.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IEventSubscriptionSetupCallbackWhenBuilder For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IEventSubscriptionSetupParallelCallbackBuilder.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IEventSetup Only(int times);
}

/// <summary>
///     Interface for setting up an event unsubscription with fluent syntax.
/// </summary>
public interface IEventUnsubscriptionSetupParallelCallbackBuilder : IEventUnsubscriptionSetupCallbackWhenBuilder
{
	/// <summary>
	///     Limits the callback to only execute for event interactions where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the event has been interacted with so far.
	/// </remarks>
	IEventUnsubscriptionSetupCallbackWhenBuilder When(Func<int, bool> predicate);
}

/// <summary>
///     Interface for setting up an event unsubscription with fluent syntax.
/// </summary>
public interface IEventUnsubscriptionSetupCallbackBuilder : IEventUnsubscriptionSetupParallelCallbackBuilder
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IEventUnsubscriptionSetupParallelCallbackBuilder InParallel();
}

/// <summary>
///     Interface for setting up an event unsubscription with fluent syntax.
/// </summary>
public interface IEventUnsubscriptionSetupCallbackWhenBuilder : IEventSetup
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IEventUnsubscriptionSetupParallelCallbackBuilder.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IEventUnsubscriptionSetupCallbackWhenBuilder For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IEventUnsubscriptionSetupParallelCallbackBuilder.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IEventSetup Only(int times);
}
