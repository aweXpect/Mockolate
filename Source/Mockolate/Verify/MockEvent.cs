using System.Linq;
using Mockolate.Checks;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which events were subscribed or unsubscribed on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockEvent<T, TMock>(IMockVerify<TMock> verify) : IMockEvent<TMock>
{
	/// <inheritdoc cref="IMockEvent{TMock}.Subscribed(string)" />
	CheckResult<TMock> IMockEvent<TMock>.Subscribed(string eventName)
		=> new(verify.Mock, verify.Interactions,
			verify.Interactions.Interactions
				.OfType<EventSubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
        $"subscribed to event {eventName.SubstringAfterLast('.')}");

	/// <inheritdoc cref="IMockEvent{TMock}.Unsubscribed(string)" />
	CheckResult<TMock> IMockEvent<TMock>.Unsubscribed(string eventName)
		=> new(verify.Mock, verify.Interactions,
			verify.Interactions.Interactions
				.OfType<EventUnsubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
        $"unsubscribed from event {eventName.SubstringAfterLast('.')}");

	/// <summary>
	///     A proxy implementation of <see cref="IMockEvent{TMock}" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockEvent<TMock> inner, IMockVerify<TMock> verify)
		: MockEvent<T, TMock>(verify), IMockEvent<TMock>
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
	public class Protected(IMockVerify<TMock> verify)
		: MockEvent<T, TMock>(verify), IMockEvent<TMock>
	{
	}
}
