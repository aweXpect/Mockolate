namespace Mockolate.Legacy.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Get results for method invocations on the mock.
/// </summary>
public interface IMockInvoked<T>
{
	/// <summary>
	///     Counts the invocations of method <paramref name="methodName" /> with matching <paramref name="parameters" />.
	/// </summary>
	VerificationResult<T> Method(string methodName, params Match.IParameter[] parameters);

	/// <summary>
	///     Counts the invocations of method <paramref name="methodName" /> with matching <paramref name="parameters" />.
	/// </summary>
	VerificationResult<T> Method(string methodName, Match.IParameters parameters);
}
#pragma warning restore S2326 // Unused type parameters should be removed
