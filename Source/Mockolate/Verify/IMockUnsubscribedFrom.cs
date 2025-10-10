namespace Mockolate.Verify;

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
