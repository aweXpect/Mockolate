namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which protected events were unsubscribed from on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifyUnsubscribedFromProtected<T>: IMockVerifyVerb<T>
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
