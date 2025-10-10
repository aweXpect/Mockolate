namespace Mockolate.Verify;

/// <summary>
///     Get results for event subscriptions on the mock.
/// </summary>
public interface IMockSubscribedTo<TMock>
{
	/// <summary>
	///     Counts the subscriptions to the event <paramref name="eventName" />.
	/// </summary>
	CheckResult<TMock> Event(string eventName);
}
