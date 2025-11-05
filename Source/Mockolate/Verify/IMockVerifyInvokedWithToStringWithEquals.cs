namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" /> when it contains a <see cref="object.ToString()" /> and <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockVerifyInvokedWithToStringWithEquals<T, TMock> : IMockVerifyInvokedWithToString<T, TMock>, IMockVerifyInvokedWithEquals<T, TMock>
{

}
#pragma warning restore S2326 // Unused type parameters should be removed
