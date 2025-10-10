using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which events were subscribed to on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockSubscribedTo<T, TMock>(IMockVerify<TMock> verify) : IMockSubscribedTo<TMock>
{
	/// <inheritdoc cref="IMockSubscribedTo{TMock}.Event(string)" />
	VerificationResult<TMock> IMockSubscribedTo<TMock>.Event(string eventName)
		=> new(verify.Mock, verify.Interactions,
			verify.Interactions.Interactions
				.OfType<EventSubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
        $"subscribed to event {eventName.SubstringAfterLast('.')}");

	/// <summary>
	///     Check which protected events were subscribed to on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockVerify<TMock> verify) : MockSubscribedTo<T, TMock>(verify)
	{
	}
}
