namespace Mockolate.Verify;

/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" /> when it contains a <see cref="object.ToString()" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithToStringWithGetHashCode<T, TMock> : IMockVerifyInvokedWithToString<T, TMock>, IMockVerifyInvokedWithGetHashCode<T, TMock>
{

}
