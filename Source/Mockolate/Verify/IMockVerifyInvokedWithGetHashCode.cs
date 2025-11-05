namespace Mockolate.Verify;

/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" /> when it contains a <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithGetHashCode<T, TMock>
{
	/// <summary>
	///     Validates the invocations for the method <see cref="object.GetHashCode()"/>.
	/// </summary>
	VerificationResult<IMockVerify<T, TMock>> GetHashCode();

}
