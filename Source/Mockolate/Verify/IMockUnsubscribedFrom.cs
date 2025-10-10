using Mockolate.Checks;

namespace Mockolate.Verify;

/// <summary>
///     Get results for event subscriptions and unsubscriptions on the mock.
/// </summary>
public interface IMockUnsubscribedFrom<TMock>
{
	/// <summary>
	///     Counts the unsubscriptions from the event <paramref name="eventName" />.
	/// </summary>
	CheckResult<TMock> Event(string eventName);
}
