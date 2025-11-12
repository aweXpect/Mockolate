namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> method.
/// </summary>
public interface IMockVerifyInvokedWithToString<T>: IMockVerifyVerb<T>
{
	/// <summary>
	///     Validates the invocations for the method <see cref="object.ToString()"/>.
	/// </summary>
	VerificationResult<T> ToString();
}
#pragma warning restore S2326 // Unused type parameters should be removed
