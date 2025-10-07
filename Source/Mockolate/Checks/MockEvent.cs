using System.Linq;
using Mockolate.Checks.Interactions;
using Mockolate.Internals;

namespace Mockolate.Checks;

/// <summary>
///     Check which events were subscribed or unsubscribed on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockEvent<T, TMock>(Checks checks, TMock mock) : IMockEvent<TMock>
{
	/// <inheritdoc cref="IMockEvent{TMock}.Subscribed(string)" />
	CheckResult<TMock> IMockEvent<TMock>.Subscribed(string eventName)
		=> new(mock, checks,
			checks.Interactions
				.OfType<EventSubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
        $"subscribed to event {eventName.SubstringAfterLast('.')}");

	/// <inheritdoc cref="IMockEvent{TMock}.Unsubscribed(string)" />
	CheckResult<TMock> IMockEvent<TMock>.Unsubscribed(string eventName)
		=> new(mock, checks,
			checks.Interactions
				.OfType<EventUnsubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
        $"unsubscribed from event {eventName.SubstringAfterLast('.')}");

	/// <summary>
	///     A proxy implementation of <see cref="IMockEvent{TMock}" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockEvent<TMock> inner, Checks checks, TMock mock)
		: MockEvent<T, TMock>(checks, mock), IMockEvent<TMock>
	{
		/// <inheritdoc cref="IMockEvent{TMock}.Subscribed(string)" />
		CheckResult<TMock> IMockEvent<TMock>.Subscribed(string eventName)
			=> inner.Subscribed(eventName);

		/// <inheritdoc cref="IMockEvent{TMock}.Unsubscribed(string)" />
		CheckResult<TMock> IMockEvent<TMock>.Unsubscribed(string eventName)
			=> inner.Unsubscribed(eventName);
	}

	/// <summary>
	///     Check which protected events were subscribed or unsubscribed on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockEvent<TMock> inner, Checks checks, TMock mock)
		: MockEvent<T, TMock>(checks, mock), IMockEvent<TMock>
	{
		/// <inheritdoc cref="IMockEvent{TMock}.Subscribed(string)" />
		CheckResult<TMock> IMockEvent<TMock>.Subscribed(string eventName)
			=> inner.Subscribed(eventName);

		/// <inheritdoc cref="IMockEvent{TMock}.Unsubscribed(string)" />
		CheckResult<TMock> IMockEvent<TMock>.Unsubscribed(string eventName)
			=> inner.Unsubscribed(eventName);
	}
}
