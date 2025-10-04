using System.Linq;

namespace Mockolate.Checks;

/// <summary>
///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T" />.
/// </summary>
public class MockEvent<T>(MockInvocations invocations) : IMockEvent
{
	/// <inheritdoc cref="IMockEvent.Subscribed(string)" />
	IInvocation[] IMockEvent.Subscribed(string eventName) => invocations.Invocations
		.OfType<EventSubscription>()
		.Where(@event => @event.Name.Equals(eventName))
		.ToArray();

	/// <inheritdoc cref="IMockEvent.Unsubscribed(string)" />
	IInvocation[] IMockEvent.Unsubscribed(string eventName) => invocations.Invocations
		.OfType<EventUnsubscription>()
		.Where(@event => @event.Name.Equals(eventName))
		.ToArray();

	/// <summary>
	///     A proxy implementation of <see cref="IMockEvent" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockEvent inner, MockInvocations invocations) : MockEvent<T>(invocations), IMockEvent
	{
		/// <inheritdoc cref="IMockEvent.Subscribed(string)" />
		IInvocation[] IMockEvent.Subscribed(string eventName)
			=> inner.Subscribed(eventName);

		/// <inheritdoc cref="IMockEvent.Unsubscribed(string)" />
		IInvocation[] IMockEvent.Unsubscribed(string eventName)
			=> inner.Unsubscribed(eventName);
	}

	/// <summary>
	///     Check which protected events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T" />.
	/// </summary>
	public class Protected(IMockEvent inner, MockInvocations invocations) : MockEvent<T>(invocations), IMockEvent
	{
		/// <inheritdoc cref="IMockEvent.Subscribed(string)" />
		IInvocation[] IMockEvent.Subscribed(string eventName)
			=> inner.Subscribed(eventName);

		/// <inheritdoc cref="IMockEvent.Unsubscribed(string)" />
		IInvocation[] IMockEvent.Unsubscribed(string eventName)
			=> inner.Unsubscribed(eventName);
	}
}
