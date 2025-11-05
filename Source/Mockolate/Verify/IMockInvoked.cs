namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Get results for method invocations on the mock.
/// </summary>
public interface IMockInvoked<TMock>
{
	/// <summary>
	///     Counts the invocations of method <paramref name="methodName" /> with matching <paramref name="parameters" />.
	/// </summary>
	VerificationResult<TMock> Method(string methodName, params With.Parameter[] parameters);

	/// <summary>
	///     Counts the invocations of method <paramref name="methodName" /> with matching <paramref name="parameters" />.
	/// </summary>
	VerificationResult<TMock> Method(string methodName, With.Parameters parameters);
}
#pragma warning restore S2326 // Unused type parameters should be removed
