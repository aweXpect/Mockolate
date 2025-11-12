namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" />, <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithToStringWithEqualsWithGetHashCode<T> : IMockVerifyInvokedWithToString<T>, IMockVerifyInvokedWithEquals<T>, IMockVerifyInvokedWithGetHashCode<T>
{

}
#pragma warning restore S2326 // Unused type parameters should be removed
