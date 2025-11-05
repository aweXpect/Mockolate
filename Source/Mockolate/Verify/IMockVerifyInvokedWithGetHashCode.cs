namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
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
#pragma warning restore S2326 // Unused type parameters should be removed
