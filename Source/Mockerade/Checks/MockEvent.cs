using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mockerade.Setup;

namespace Mockerade.Checks;

/// <summary>
///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T"/>.
/// </summary>
public class MockEvent<T>(MockInvocations invocations) : IMockEvent
{
	/// <summary>
	///     A proxy implementation of <see cref="IMockEvent"/> that forwards all calls to the provided <paramref name="inner"/> instance.
	/// </summary>
	public class Proxy(IMockEvent inner, MockInvocations invocations) : MockEvent<T>(invocations), IMockEvent
	{
		/// <inheritdoc cref="IMockEvent.Subscribed(string)" />
		Invocation[] IMockEvent.Subscribed(string propertyName)
			=> inner.Subscribed(propertyName);

		/// <inheritdoc cref="IMockEvent.Unsubscribed(string)" />
		Invocation[] IMockEvent.Unsubscribed(string propertyName)
			=> inner.Unsubscribed(propertyName);
	}

	/// <summary>
	///     Check which protected events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public class Protected(IMockEvent inner, MockInvocations invocations) : MockEvent<T>(invocations), IMockEvent
	{
		/// <inheritdoc cref="IMockEvent.Subscribed(string)" />
		Invocation[] IMockEvent.Subscribed(string propertyName)
			=> inner.Subscribed(propertyName);

		/// <inheritdoc cref="IMockEvent.Unsubscribed(string)" />
		Invocation[] IMockEvent.Unsubscribed(string propertyName)
			=> inner.Unsubscribed(propertyName);
	}

	/// <inheritdoc cref="IMockEvent.Subscribed(string)"/>
	Invocation[] IMockEvent.Subscribed(string propertyName)
	{
		return invocations.Invocations
			.OfType<EventSubscription>()
			.Where(@event => @event.Name.Equals(propertyName))
			.ToArray();
	}

	/// <inheritdoc cref="IMockEvent.Unsubscribed(string)"/>
	Invocation[] IMockEvent.Unsubscribed(string propertyName)
	{
		return invocations.Invocations
			.OfType<EventUnsubscription>()
			.Where(@event => @event.Name.Equals(propertyName))
			.ToArray();
	}
}
