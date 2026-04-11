using System.Diagnostics;

namespace Mockolate.Verify;

/// <summary>
///     Verifications on an event.
/// </summary>
#if RELEASE
[DebuggerNonUserCode]
#endif
public class VerificationEventResult<TSubject>
{
	private readonly string _name;
	private readonly MockRegistry _mockRegistry;
	private readonly TSubject _subject;

	/// <inheritdoc cref="VerificationEventResult{TSubject}" />
	public VerificationEventResult(TSubject subject, MockRegistry mockRegistry, string name)
	{
		_subject = subject;
		_mockRegistry = mockRegistry;
		_name = name;
	}

	/// <summary>
	///     Verifies the subscriptions for the event.
	/// </summary>
	public VerificationResult<TSubject> Subscribed()
		=> _mockRegistry.SubscribedTo(_subject, _name);

	/// <summary>
	///     Verifies the unsubscriptions from the event.
	/// </summary>
	public VerificationResult<TSubject> Unsubscribed()
		=> _mockRegistry.UnsubscribedFrom(_subject, _name);
}
