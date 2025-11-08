namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Get results for indexer set access on the mock.
/// </summary>
public interface IMockSetIndexer<TMock>
{
	/// <summary>
	///     Counts the setter accesses of the indexer with matching <paramref name="parameters" /> to the given
	///     <paramref name="value" />.
	/// </summary>
	VerificationResult<TMock> Set(Match.IParameter? value, params Match.IParameter?[] parameters);
}
#pragma warning restore S2326 // Unused type parameters should be removed
