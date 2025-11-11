using Mockolate.Verify;

namespace Mockolate.V2;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" /> when it contains a <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockVerifyInvokedWithEquals<T>
{
	/// <summary>
	///     Validates the invocations for the method <see cref="object.Equals(object)"/> with the given <paramref name="obj"/>.
	/// </summary>
	VerificationResult<IMockVerify<T>> Equals(Match.IParameter<object>? obj);
}
#pragma warning restore S2326 // Unused type parameters should be removed
