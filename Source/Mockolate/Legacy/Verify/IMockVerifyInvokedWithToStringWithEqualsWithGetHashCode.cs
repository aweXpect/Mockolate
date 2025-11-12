namespace Mockolate.Legacy.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" /> when it contains a <see cref="object.ToString()" />, <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithToStringWithEqualsWithGetHashCode<T, TMock> : IMockVerifyInvokedWithToString<T, TMock>, IMockVerifyInvokedWithEquals<T, TMock>, IMockVerifyInvokedWithGetHashCode<T, TMock>
{

}
#pragma warning restore S2326 // Unused type parameters should be removed
