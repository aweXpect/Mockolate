namespace Mockolate.Verify;

/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" /> when it contains a <see cref="object.ToString()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithToString<T, TMock>
{
	/// <summary>
	///     Validates the invocations for the method <see cref="object.ToString()"/>.
	/// </summary>
	VerificationResult<IMockVerify<T, TMock>> ToString();

}
