namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithGetHashCode<T>: IMockVerifyVerb<T>
{
	/// <summary>
	///     Validates the invocations for the method <see cref="object.GetHashCode()"/>.
	/// </summary>
	VerificationResult<T> GetHashCode();
}
#pragma warning restore S2326 // Unused type parameters should be removed
