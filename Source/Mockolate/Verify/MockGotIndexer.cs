using System.Linq;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     Check which indexers got read on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockGotIndexer<T, TMock>(MockVerify<T, TMock> verify) : IMockGotIndexer<MockVerify<T, TMock>>
{
	/// <inheritdoc cref="IMockGotIndexer{TMock}.Got(With.Parameter?[])" />
	VerificationResult<MockVerify<T, TMock>> IMockGotIndexer<MockVerify<T, TMock>>.Got(params With.Parameter?[] parameters)
	{
		MockInteractions interactions = ((IMockVerify<TMock>)verify).Interactions;
		return new(verify, interactions,
			interactions.Interactions
				.OfType<IndexerGetterAccess>()
				.Where(indexer => indexer.Parameters.Length == parameters.Length &&
				!parameters.Where((parameter, i) => parameter is null
					? indexer.Parameters[i] is not null
					: !parameter.Matches(indexer.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
		$"got indexer {string.Join(", ", parameters.Select(x => x?.ToString() ?? "null"))}");
	}

	/// <summary>
	///     Check which protected indexers got read on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(MockVerify<T, TMock> verify) : MockGotIndexer<T, TMock>(verify)
	{
	}
}
