using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which events were unsubscribed from on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockUnsubscribedFrom<T, TMock>(MockVerify<T, TMock> verify) : IMockUnsubscribedFrom<MockVerify<T, TMock>>
{
	/// <inheritdoc cref="IMockUnsubscribedFrom{TMock}.Event(string)" />
	VerificationResult<MockVerify<T, TMock>> IMockUnsubscribedFrom<MockVerify<T, TMock>>.Event(string eventName)
	{
		MockInteractions interactions = ((IMockVerify<TMock>)verify).Interactions;
		return new(verify, interactions,
			interactions.Interactions
				.OfType<EventUnsubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
		$"unsubscribed from event {eventName.SubstringAfterLast('.')}");
	}

	/// <summary>
	///     Check which protected events were unsubscribed from on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(MockVerify<T, TMock> verify) : MockUnsubscribedFrom<T, TMock>(verify)
	{
	}
}
