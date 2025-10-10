using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which indexers got read on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockGotIndexer<T, TMock>(IMockVerify<TMock> verify) : IMockGotIndexer<TMock>
{
	/// <inheritdoc cref="IMockGotIndexer{TMock}.Got(With.Parameter?[])" />
	VerificationResult<TMock> IMockGotIndexer<TMock>.Got(params With.Parameter?[] parameters)
		=> new(verify.Mock, verify.Interactions,
			verify.Interactions.Interactions
				.OfType<IndexerGetterAccess>()
				.Where(indexer => indexer.Parameters.Length == parameters.Length &&
				!parameters.Where((parameter, i) => parameter is null
					? indexer.Parameters[i] is null
					: !parameter.Matches(indexer.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
        $"got indexer {string.Join(", ", parameters.Select(x => x?.ToString() ?? "null"))}");

	/// <summary>
	///     Check which protected indexers got read on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockVerify<TMock> verify) : MockGotIndexer<T, TMock>(verify)
	{
	}
}
