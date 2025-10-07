namespace Mockolate.Checks;

/// <summary>
///     Get results for event subscriptions and unsubscriptions on the mock.
/// </summary>
public interface IMockEvent<TMock>
{
	/// <summary>
	///     Counts the subscriptions to the event <paramref name="eventName" />.
	/// </summary>
	CheckResult<TMock> Subscribed(string eventName);

	/// <summary>
	///     Counts the unsubscriptions from the event <paramref name="eventName" />.
	/// </summary>
	CheckResult<TMock> Unsubscribed(string eventName);
}
