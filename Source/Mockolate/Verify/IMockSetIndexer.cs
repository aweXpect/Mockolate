namespace Mockolate.Verify;

/// <summary>
///     Get results for indexer set access on the mock.
/// </summary>
public interface IMockSetIndexer<TMock>
{
	/// <summary>
	///     Counts the setter accesses of the indexer with matching <paramref name="parameters" /> to the given <paramref name="value"/>.
	/// </summary>
	VerificationResult<TMock> Set(With.Parameter? value, params With.Parameter?[] parameters);
}
