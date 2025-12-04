using Mockolate.Verify;

namespace Mockolate;

public partial class Mock<T> : IMockVerify<T>,
	IMockVerifyInvoked<T>, IMockVerifyInvokedProtected<T>,
	IMockVerifyInvokedWithToStringWithEqualsWithGetHashCode<T>, IMockVerifyInvokedWithEqualsWithGetHashCode<T>,
	IMockVerifyInvokedWithToStringWithGetHashCode<T>, IMockVerifyInvokedWithToStringWithEquals<T>,
	IMockVerifyGot<T>, IMockVerifyGotProtected<T>,
	IMockVerifySet<T>, IMockVerifySetProtected<T>,
	IMockVerifyGotIndexer<T>, IMockVerifyGotIndexerProtected<T>,
	IMockVerifySetIndexer<T>, IMockVerifySetIndexerProtected<T>,
	IMockVerifySubscribedTo<T>, IMockVerifySubscribedToProtected<T>,
	IMockVerifyUnsubscribedFrom<T>, IMockVerifyUnsubscribedFromProtected<T>
{
	/// <inheritdoc cref="IMockVerify{T}.ThatAllInteractionsAreVerified()" />
	bool IMockVerify<T>.ThatAllInteractionsAreVerified()
		=> Interactions.GetUnverifiedInteractions().Count == 0;

	/// <inheritdoc cref="IMockVerify{T}.ThatAllSetupsAreUsed" />
	bool IMockVerify<T>.ThatAllSetupsAreUsed()
		=> Registrations.GetUnusedSetups(Interactions).Count == 0;

	/// <inheritdoc cref="IMockVerifyInvokedWithToString{T}.ToString()" />
	VerificationResult<T> IMockVerifyInvokedWithToString<T>.ToString()
		=> Registrations.Method(Subject, Registrations.Prefix + ".ToString");

	/// <inheritdoc cref="IMockVerifyInvokedWithEquals{T}.Equals(Match.IParameter{object})" />
	VerificationResult<T> IMockVerifyInvokedWithEquals<T>.Equals(Match.IParameter<object>? obj)
		=> Registrations.Method(Subject, Registrations.Prefix + ".Equals",
			new Match.NamedParameter("obj", (Match.IParameter)(obj ?? Match.Null<object>())));

	/// <inheritdoc cref="IMockVerifyInvokedWithGetHashCode{T}.GetHashCode()" />
	VerificationResult<T> IMockVerifyInvokedWithGetHashCode<T>.GetHashCode()
		=> Registrations.Method(Subject, Registrations.Prefix + ".GetHashCode");

	/// <summary>
	///     Counts the invocations of method <paramref name="methodName" /> with matching <paramref name="parameters" />.
	/// </summary>
	public VerificationResult<T> Method(string methodName, params Match.NamedParameter[] parameters)
		=> Registrations.Method(Subject, methodName, parameters);

	/// <summary>
	///     Counts the invocations of method <paramref name="methodName" /> with matching <paramref name="parameters" />.
	/// </summary>
	public VerificationResult<T> Method(string methodName, Match.IParameters parameters)
		=> Registrations.Method(Subject, methodName, parameters);

	/// <summary>
	///     Counts the getter accesses of property <paramref name="propertyName" />.
	/// </summary>
	public VerificationResult<T> Property(string propertyName)
		=> Registrations.Property(Subject, propertyName);

	/// <summary>
	///     Counts the setter accesses of property <paramref name="propertyName" />
	///     with the matching <paramref name="value" />.
	/// </summary>
	public VerificationResult<T> Property(string propertyName, Match.IParameter value)
		=> Registrations.Property(Subject, propertyName, value);

	/// <summary>
	///     Counts the getter accesses of the indexer with matching <paramref name="parameters" />.
	/// </summary>
	public VerificationResult<T> GotIndexer(params Match.NamedParameter?[] parameters)
		=> Registrations.Indexer(Subject, parameters);

	/// <summary>
	///     Counts the setter accesses of the indexer with matching <paramref name="parameters" /> to the given
	///     <paramref name="value" />.
	/// </summary>
	public VerificationResult<T> SetIndexer(Match.IParameter? value, params Match.NamedParameter?[] parameters)
		=> Registrations.Indexer(Subject, value, parameters);

	/// <summary>
	///     Counts the subscriptions to the event <paramref name="eventName" />.
	/// </summary>
	public VerificationResult<T> SubscribedTo(string eventName)
		=> Registrations.SubscribedTo(Subject, eventName);

	/// <summary>
	///     Counts the unsubscriptions from the event <paramref name="eventName" />.
	/// </summary>
	public VerificationResult<T> UnsubscribedFrom(string eventName)
		=> Registrations.UnsubscribedFrom(Subject, eventName);
}
