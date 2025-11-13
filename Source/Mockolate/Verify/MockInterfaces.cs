namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Verifies the <see cref="Interactions" /> with the mocked subject in the <typeparamref name="T" />.
/// </summary>
public interface IMockVerify<out T> : IInteractiveMock<T>
{
	/// <summary>
	///     Gets a value indicating whether all expected interactions have been verified.
	/// </summary>
	bool ThatAllInteractionsAreVerified();
}
/// <summary>
///     Check which properties got read on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifyGot<out T> : IInteractiveMock<T>;
/// <summary>
///     Check which indexers got read on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifyGotIndexer<out T> : IInteractiveMock<T>;
/// <summary>
///     Check which protected indexers got read on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifyGotIndexerProtected<out T> : IInteractiveMock<T>;
/// <summary>
///     Check which protected properties got read on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifyGotProtected<out T> : IInteractiveMock<T>;
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" />.
/// </summary>
public interface IMockVerifyInvoked<T> : IInteractiveMock<T>;
/// <summary>
///     Check which protected methods got invoked on the mocked instance for <typeparamref name="T" />.
/// </summary>
public interface IMockVerifyInvokedProtected<out T> : IInteractiveMock<T>;
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockVerifyInvokedWithEquals<T> : IInteractiveMock<T>
{
	/// <summary>
	///     Validates the invocations for the method <see cref="object.Equals(object)"/> with the given <paramref name="obj"/>.
	/// </summary>
	VerificationResult<T> Equals(Match.IParameter<object>? obj);
}
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithEqualsWithGetHashCode<T> : IMockVerifyInvokedWithEquals<T>, IMockVerifyInvokedWithGetHashCode<T>;
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithGetHashCode<T> : IInteractiveMock<T>
{
	/// <summary>
	///     Validates the invocations for the method <see cref="object.GetHashCode()"/>.
	/// </summary>
	VerificationResult<T> GetHashCode();
}
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithToString<T> : IInteractiveMock<T>
{
	/// <summary>
	///     Validates the invocations for the method <see cref="object.ToString()"/>.
	/// </summary>
	VerificationResult<T> ToString();
}
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> and <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockVerifyInvokedWithToStringWithEquals<T> : IMockVerifyInvokedWithToString<T>, IMockVerifyInvokedWithEquals<T>;
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" />, <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithToStringWithEqualsWithGetHashCode<T> : IMockVerifyInvokedWithToString<T>, IMockVerifyInvokedWithEquals<T>, IMockVerifyInvokedWithGetHashCode<T>;
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithToStringWithGetHashCode<T> : IMockVerifyInvokedWithToString<T>, IMockVerifyInvokedWithGetHashCode<T>;
/// <summary>
///     Check which properties were set on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifySet<out T> : IInteractiveMock<T>;
/// <summary>
///     Check which indexers were set on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifySetIndexer<out T> : IInteractiveMock<T>;
/// <summary>
///     Check which protected indexers were set on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifySetIndexerProtected<out T> : IInteractiveMock<T>;
/// <summary>
///     Check which protected properties were set on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifySetProtected<out T> : IInteractiveMock<T>;
/// <summary>
///     Check which events were subscribed to on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifySubscribedTo<out T> : IInteractiveMock<T>;
/// <summary>
///     Check which protected events were subscribed to on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifySubscribedToProtected<out T> : IInteractiveMock<T>;
/// <summary>
///     Check which events were unsubscribed from on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifyUnsubscribedFrom<out T> : IInteractiveMock<T>;
/// <summary>
///     Check which protected events were unsubscribed from on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifyUnsubscribedFromProtected<out T> : IInteractiveMock<T>;
#pragma warning restore S2326 // Unused type parameters should be removed
