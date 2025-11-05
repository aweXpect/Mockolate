namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" />.
/// </summary>
public interface IMockVerifyInvoked<T, out TMock>
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
