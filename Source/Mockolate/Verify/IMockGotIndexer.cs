namespace Mockolate.Verify;

/// <summary>
///     Get results for property get access on the mock.
/// </summary>
public interface IMockGotIndexer<TMock>
{
	/// <summary>
	///     Counts the getter accesses of the indexer with matching <paramref name="parameters" />.
	/// </summary>
	VerificationResult<TMock> Got(params With.Parameter?[] parameters);
}
