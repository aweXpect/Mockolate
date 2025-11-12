namespace Mockolate.Legacy.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
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
#pragma warning restore S2326 // Unused type parameters should be removed
