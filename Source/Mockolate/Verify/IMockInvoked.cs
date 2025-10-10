namespace Mockolate.Verify;

/// <summary>
///     Get results for method invocations on the mock.
/// </summary>
public interface IMockInvoked<TMock>
{
	/// <summary>
	///     Counts the invocations of method <paramref name="methodName" /> with matching <paramref name="parameters" />.
	/// </summary>
	VerificationResult<TMock> Method(string methodName, params With.Parameter[] parameters);
}
