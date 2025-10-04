namespace Mockolate.Checks;

/// <summary>
///     Allows registration of <see cref="IInvocation" /> in the mock.
/// </summary>
public interface IMockEvent
{
	/// <summary>
	///     Counts the invocations for the subscription to an event with the given <paramref name="eventName" />.
	/// </summary>
	IInvocation[] Subscribed(string eventName);

	/// <summary>
	///     Counts the invocations for the unsubscription from an event with the given <paramref name="eventName" />.
	/// </summary>
	IInvocation[] Unsubscribed(string eventName);
}
