namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> and <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockVerifyInvokedWithToStringWithEquals<T> : IMockVerifyInvokedWithToString<T>, IMockVerifyInvokedWithEquals<T>
{

}
#pragma warning restore S2326 // Unused type parameters should be removed
