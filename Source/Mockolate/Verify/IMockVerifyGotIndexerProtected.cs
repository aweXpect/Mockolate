namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which protected indexers got read on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifyGotIndexerProtected<T>: IMockVerifyVerb<T>
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
