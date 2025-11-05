namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which protected properties got read on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public interface IMockVerifyGotProtected<T, out TMock>
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
