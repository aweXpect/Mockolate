using System.Diagnostics;

namespace Mockolate.Verify;

/// <summary>
///     Verifications on an event.
/// </summary>
[DebuggerNonUserCode]
public class VerificationEventResult<TSubject>
{
	private readonly string _name;
	private readonly MockRegistry _registrations;
	private readonly TSubject _subject;

	/// <inheritdoc cref="VerificationEventResult{TSubject}" />
	public VerificationEventResult(TSubject subject, MockRegistry registrations, string name)
	{
		_subject = subject;
		_registrations = registrations;
		_name = name;
	}

	/// <summary>
	///     Verifies the subscriptions for the event.
	/// </summary>
	public VerificationResult<TSubject> Subscribed()
		=> _registrations.SubscribedTo(_subject, _name);

	/// <summary>
	///     Verifies the unsubscriptions from the event.
	/// </summary>
	public VerificationResult<TSubject> Unsubscribed()
		=> _registrations.UnsubscribedFrom(_subject, _name);
}
