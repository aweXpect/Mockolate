namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Get results for event unsubscriptions on the mock.
/// </summary>
public interface IMockUnsubscribedFrom<TMock>
{
	/// <summary>
	///     Counts the unsubscriptions from the event <paramref name="eventName" />.
	/// </summary>
	VerificationResult<TMock> Event(string eventName);
}
#pragma warning restore S2326 // Unused type parameters should be removed
