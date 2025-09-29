namespace Mockerade.Checks;

/// <summary>
///     Allows registration of <see cref="Invocation" /> in the mock.
/// </summary>
public interface IMockEvent
{
	/// <summary>
	/// Counts the invocations for the subscription to an event with the given <paramref name="eventName"/>.
	/// </summary>
	Invocation[] Subscribed(string eventName);

	/// <summary>
	/// Counts the invocations for the unsubscription from an event with the given <paramref name="eventName"/>.
	/// </summary>
	Invocation[] Unsubscribed(string eventName);
}
