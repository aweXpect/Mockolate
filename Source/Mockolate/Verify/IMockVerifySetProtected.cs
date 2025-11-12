namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which protected properties were set on the mocked instance <typeparamref name="T" />.
/// </summary>
public interface IMockVerifySetProtected<T>: IMockVerifyVerb<T>
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
