using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which events were subscribed or unsubscribed on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockSubscribedTo<T, TMock>(IMockVerify<TMock> verify) : IMockSubscribedTo<TMock>
{
	/// <inheritdoc cref="IMockSubscribedTo{TMock}.Event(string)" />
	CheckResult<TMock> IMockSubscribedTo<TMock>.Event(string eventName)
		=> new(verify.Mock, verify.Interactions,
			verify.Interactions.Interactions
				.OfType<EventSubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
        $"subscribed to event {eventName.SubstringAfterLast('.')}");

	/// <summary>
	///     A proxy implementation of <see cref="IMockSubscribedTo{TMock}" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockSubscribedTo<TMock> inner, IMockVerify<TMock> verify)
		: MockSubscribedTo<T, TMock>(verify), IMockSubscribedTo<TMock>
	{
		/// <inheritdoc cref="IMockSubscribedTo{TMock}.Event(string)" />
		CheckResult<TMock> IMockSubscribedTo<TMock>.Event(string eventName)
			=> inner.Event(eventName);
	}

	/// <summary>
	///     Check which protected events were subscribed or unsubscribed on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockVerify<TMock> verify)
		: MockSubscribedTo<T, TMock>(verify), IMockSubscribedTo<TMock>
	{
	}
}
