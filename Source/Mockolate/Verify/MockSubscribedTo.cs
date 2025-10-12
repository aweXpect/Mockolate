using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which events were subscribed to on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockSubscribedTo<T, TMock>(MockVerify<T, TMock> verify) : IMockSubscribedTo<MockVerify<T, TMock>>
{
	/// <inheritdoc cref="IMockSubscribedTo{TMock}.Event(string)" />
	VerificationResult<MockVerify<T, TMock>> IMockSubscribedTo<MockVerify<T, TMock>>.Event(string eventName)
	{
		MockInteractions interactions = ((IMockVerify<TMock>)verify).Interactions;
		return new(verify, interactions,
			interactions.Interactions
				.OfType<EventSubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
		$"subscribed to event {eventName.SubstringAfterLast('.')}");
	}

	/// <summary>
	///     Check which protected events were subscribed to on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(MockVerify<T, TMock> verify) : MockSubscribedTo<T, TMock>(verify)
	{
	}
}
