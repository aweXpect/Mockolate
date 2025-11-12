namespace Mockolate.Legacy.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Get results for indexer get access on the mock.
/// </summary>
public interface IMockGotIndexer<TMock>
{
	/// <summary>
	///     Counts the getter accesses of the indexer with matching <paramref name="parameters" />.
	/// </summary>
	VerificationResult<TMock> Got(params Match.IParameter?[] parameters);
}
#pragma warning restore S2326 // Unused type parameters should be removed
