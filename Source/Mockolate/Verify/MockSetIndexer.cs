using System.Linq;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     Check which indexers were set on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockSetIndexer<T, TMock>(MockVerify<T, TMock> verify) : IMockSetIndexer<MockVerify<T, TMock>>
{
	internal MockVerify<T, TMock> Verify { get; } = verify;

	/// <inheritdoc cref="IMockSetIndexer{TMock}.Set(With.Parameter?, With.Parameter?[])" />
	VerificationResult<MockVerify<T, TMock>> IMockSetIndexer<MockVerify<T, TMock>>.Set(With.Parameter? value, params With.Parameter?[] parameters)
	{
		MockInteractions interactions = ((IMockVerify<TMock>)Verify).Interactions;
		return new(Verify, interactions,
			interactions.Interactions
				.OfType<IndexerSetterAccess>()
				.Where(indexer => indexer.Parameters.Length == parameters.Length &&
				(value is null ? indexer.Value is null : value!.Matches(indexer.Value)) &&
				!parameters.Where((parameter, i) => parameter is null
					? indexer.Parameters[i] is not null
					: !parameter.Matches(indexer.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
		$"set indexer {string.Join(", ", parameters.Select(x => x?.ToString() ?? "null"))} to value {(value?.ToString() ?? "null")}");
	}
}
