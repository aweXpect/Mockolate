namespace Mockolate.Verify;

/// <summary>
///     Verifications on an event.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public class VerificationEventResult<TSubject>
{
	private const int NoMemberId = -1;

	private readonly string _name;
	private readonly MockRegistry _mockRegistry;
	private readonly TSubject _subject;
	private readonly int _subscribeMemberId;
	private readonly int _unsubscribeMemberId;

	/// <inheritdoc cref="VerificationEventResult{TSubject}" />
	public VerificationEventResult(TSubject subject, MockRegistry mockRegistry, string name)
		: this(subject, mockRegistry, NoMemberId, NoMemberId, name)
	{
	}

	/// <summary>
	///     Member-id-keyed constructor used by generated mocks to enable per-member fast Verify walks.
	/// </summary>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="mockRegistry">The mock registry holding the recorded interactions.</param>
	/// <param name="subscribeMemberId">Member id of the event subscribe, or <c>-1</c> when unknown.</param>
	/// <param name="unsubscribeMemberId">Member id of the event unsubscribe, or <c>-1</c> when unknown.</param>
	/// <param name="name">The simple event name.</param>
	public VerificationEventResult(TSubject subject, MockRegistry mockRegistry,
		int subscribeMemberId, int unsubscribeMemberId, string name)
	{
		_subject = subject;
		_mockRegistry = mockRegistry;
		_subscribeMemberId = subscribeMemberId;
		_unsubscribeMemberId = unsubscribeMemberId;
		_name = name;
	}

	/// <summary>
	///     Verifies the subscriptions for the event.
	/// </summary>
	public VerificationResult<TSubject> Subscribed()
		=> _mockRegistry.SubscribedTo(_subject, _subscribeMemberId, _name);

	/// <summary>
	///     Verifies the unsubscriptions from the event.
	/// </summary>
	public VerificationResult<TSubject> Unsubscribed()
		=> _mockRegistry.UnsubscribedFrom(_subject, _unsubscribeMemberId, _name);
}
