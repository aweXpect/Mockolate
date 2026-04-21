using System;
using System.Reflection;

namespace Mockolate.Setup;

/// <summary>
///     Fluent surface for observing subscription lifecycle of a mocked event.
/// </summary>
/// <remarks>
///     Reached via <c>sut.Mock.Setup.EventName</c>. Use <see cref="OnSubscribed" /> to attach callbacks that fire
///     when a handler is added via <c>+=</c>, and <see cref="OnUnsubscribed" /> for handlers removed via <c>-=</c>.
///     To actually trigger the event on subscribers, use <c>sut.Mock.Raise.EventName(...)</c> instead.
/// </remarks>
public interface IEventSetup
{
	/// <summary>
	///     Attaches callbacks that fire whenever a handler is subscribed to the event (the <c>add</c> accessor).
	/// </summary>
	IEventSubscriptionSetup OnSubscribed { get; }

	/// <summary>
	///     Attaches callbacks that fire whenever a handler is unsubscribed from the event (the <c>remove</c>
	///     accessor).
	/// </summary>
	IEventUnsubscriptionSetup OnUnsubscribed { get; }
}

/// <summary>
///     Fluent surface for attaching side-effects to an event's <c>add</c> accessor.
/// </summary>
/// <remarks>
///     Each <c>Do</c> registers a callback that fires on every handler subscription. Chain multiple <c>Do</c> calls
///     to form a sequence; chain <c>.InParallel()</c>, <c>.When(predicate)</c>, <c>.For(n)</c>, <c>.Only(n)</c>,
///     <c>.OnlyOnce()</c>, <c>.Forever()</c> on the returned builders to control repetition and gating.
/// </remarks>
public interface IEventSubscriptionSetup
{
	/// <summary>
	///     Fires <paramref name="callback" /> whenever a handler subscribes to the event.
	/// </summary>
	/// <example>
	///     <code>
	///     sut.Mock.Setup.UsersChanged.OnSubscribed.Do(() =&gt; Console.WriteLine("subscribed!"));
	///     </code>
	/// </example>
	IEventSubscriptionSetupCallbackBuilder Do(Action callback);

	/// <summary>
	///     Fires <paramref name="callback" /> whenever a handler subscribes, passing the subscriber's target object
	///     (<see langword="null" /> for static methods) and <see cref="MethodInfo" />.
	/// </summary>
	/// <remarks>
	///     Useful for diagnostics or for asserting which specific method on which target was wired up.
	/// </remarks>
	IEventSubscriptionSetupCallbackBuilder Do(Action<object?, MethodInfo> callback);

	/// <summary>
	///     Switches the mock's current scenario to <paramref name="scenario" /> whenever a handler subscribes.
	/// </summary>
	/// <param name="scenario">The name of the scenario to transition to.</param>
	IEventSubscriptionSetupParallelCallbackBuilder TransitionTo(string scenario);
}

/// <summary>
///     Fluent surface for attaching side-effects to an event's <c>remove</c> accessor.
/// </summary>
/// <remarks>
///     Mirror of <see cref="IEventSubscriptionSetup" /> for unsubscriptions: each <c>Do</c> registers a callback that
///     fires when a handler is removed via <c>-=</c>; the same builder operators (<c>.InParallel()</c>,
///     <c>.When(predicate)</c>, <c>.For(n)</c>, <c>.Only(n)</c>, <c>.OnlyOnce()</c>, <c>.Forever()</c>) apply.
/// </remarks>
public interface IEventUnsubscriptionSetup
{
	/// <summary>
	///     Fires <paramref name="callback" /> whenever a handler unsubscribes from the event.
	/// </summary>
	IEventUnsubscriptionSetupCallbackBuilder Do(Action callback);

	/// <summary>
	///     Fires <paramref name="callback" /> whenever a handler unsubscribes, passing the handler's target object
	///     (<see langword="null" /> for static methods) and <see cref="MethodInfo" />.
	/// </summary>
	IEventUnsubscriptionSetupCallbackBuilder Do(Action<object?, MethodInfo> callback);

	/// <summary>
	///     Switches the mock's current scenario to <paramref name="scenario" /> whenever a handler unsubscribes.
	/// </summary>
	/// <param name="scenario">The name of the scenario to transition to.</param>
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
