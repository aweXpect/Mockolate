namespace Mockolate.Verify;

/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" /> when it contains a <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithEqualsWithGetHashCode<T, TMock> : IMockVerifyInvokedWithEquals<T, TMock>, IMockVerifyInvokedWithGetHashCode<T, TMock>
{

}
