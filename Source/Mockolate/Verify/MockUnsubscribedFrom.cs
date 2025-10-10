using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which events were unsubscribed from on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockUnsubscribedFrom<T, TMock>(IMockVerify<TMock> verify) : IMockUnsubscribedFrom<TMock>
{
	/// <inheritdoc cref="IMockUnsubscribedFrom{TMock}.Event(string)" />
	VerificationResult<TMock> IMockUnsubscribedFrom<TMock>.Event(string eventName)
		=> new(verify.Mock, verify.Interactions,
			verify.Interactions.Interactions
				.OfType<EventUnsubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
        $"unsubscribed from event {eventName.SubstringAfterLast('.')}");

	/// <summary>
	///     Check which protected events were unsubscribed from on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockVerify<TMock> verify) : MockUnsubscribedFrom<T, TMock>(verify)
	{
	}
}
