namespace Mockolate.Verify;

/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" /> when it contains a <see cref="object.ToString()" /> and <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockVerifyInvokedWithToStringWithEquals<T, TMock> : IMockVerifyInvokedWithToString<T, TMock>, IMockVerifyInvokedWithEquals<T, TMock>
{

}
