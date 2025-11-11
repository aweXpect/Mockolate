using Mockolate.Verify;

namespace Mockolate.V2;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithGetHashCode<T>
{
	/// <summary>
	///     Validates the invocations for the method <see cref="object.GetHashCode()"/>.
	/// </summary>
	VerificationResult<IMockVerify<T>> GetHashCode();
}
#pragma warning restore S2326 // Unused type parameters should be removed
