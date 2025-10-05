using System.Linq;
using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T" />.
/// </summary>
public class MockEvent<T>(MockChecks checks) : IMockEvent
{
	/// <inheritdoc cref="IMockEvent.Subscribed(string)" />
	CheckResult IMockEvent.Subscribed(string eventName) => new(checks,
		checks.Interactions
			.OfType<EventSubscription>()
			.Where(@event => @event.Name.Equals(eventName))
			.ToArray());

	/// <inheritdoc cref="IMockEvent.Unsubscribed(string)" />
	CheckResult IMockEvent.Unsubscribed(string eventName) => new(checks,
		checks.Interactions
			.OfType<EventUnsubscription>()
			.Where(@event => @event.Name.Equals(eventName))
			.ToArray());

	/// <summary>
	///     A proxy implementation of <see cref="IMockEvent" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockEvent inner, MockChecks invocations) : MockEvent<T>(invocations), IMockEvent
	{
		/// <inheritdoc cref="IMockEvent.Subscribed(string)" />
		CheckResult IMockEvent.Subscribed(string eventName)
			=> inner.Subscribed(eventName);

		/// <inheritdoc cref="IMockEvent.Unsubscribed(string)" />
		CheckResult IMockEvent.Unsubscribed(string eventName)
			=> inner.Unsubscribed(eventName);
	}

	/// <summary>
	///     Check which protected events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T" />.
	/// </summary>
	public class Protected(IMockEvent inner, MockChecks invocations) : MockEvent<T>(invocations), IMockEvent
	{
		/// <inheritdoc cref="IMockEvent.Subscribed(string)" />
		CheckResult IMockEvent.Subscribed(string eventName)
			=> inner.Subscribed(eventName);

		/// <inheritdoc cref="IMockEvent.Unsubscribed(string)" />
		CheckResult IMockEvent.Unsubscribed(string eventName)
			=> inner.Unsubscribed(eventName);
	}
}
