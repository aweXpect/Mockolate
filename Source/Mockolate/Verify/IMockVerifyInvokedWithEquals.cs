namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" /> when it contains a <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockVerifyInvokedWithEquals<T, TMock>
{
	/// <summary>
	///     Validates the invocations for the method <see cref="object.Equals(object)"/> with the given <paramref name="obj"/>.
	/// </summary>
	VerificationResult<IMockVerify<T, TMock>> Equals(With.Parameter<object>? obj);
}
#pragma warning restore S2326 // Unused type parameters should be removed
