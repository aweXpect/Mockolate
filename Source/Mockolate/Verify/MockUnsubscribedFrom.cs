using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which events were unsubscribed from on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockUnsubscribedFrom<T, TMock>(MockVerify<T, TMock> verify) : IMockUnsubscribedFrom<MockVerify<T, TMock>>
{
	internal MockVerify<T, TMock> Verify { get; } = verify;

	/// <inheritdoc cref="IMockUnsubscribedFrom{TMock}.Event(string)" />
	VerificationResult<MockVerify<T, TMock>> IMockUnsubscribedFrom<MockVerify<T, TMock>>.Event(string eventName)
	{
		MockInteractions interactions = ((IMockVerify<TMock>)Verify).Interactions;
		return new(Verify, interactions,
			interactions.Interactions
				.OfType<EventUnsubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
		$"unsubscribed from event {eventName.SubstringAfterLast('.')}");
	}
}
