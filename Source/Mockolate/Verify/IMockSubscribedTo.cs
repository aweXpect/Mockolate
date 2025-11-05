namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Get results for event subscriptions on the mock.
/// </summary>
public interface IMockSubscribedTo<TMock>
{
	/// <summary>
	///     Counts the subscriptions to the event <paramref name="eventName" />.
	/// </summary>
	VerificationResult<TMock> Event(string eventName);
}
#pragma warning restore S2326 // Unused type parameters should be removed
