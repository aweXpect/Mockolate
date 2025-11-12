namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" />.
/// </summary>
public interface IMockVerifyInvoked<T> : IMockVerifyVerb<T>
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
