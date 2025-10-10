using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which indexers were set on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockSetIndexer<T, TMock>(IMockVerify<TMock> verify) : IMockSetIndexer<TMock>
{
	/// <inheritdoc cref="IMockSetIndexer{TMock}.Set(With.Parameter?, With.Parameter?[])" />
	VerificationResult<TMock> IMockSetIndexer<TMock>.Set(With.Parameter? value, params With.Parameter?[] parameters)
		=> new(verify.Mock, verify.Interactions,
			verify.Interactions.Interactions
				.OfType<IndexerSetterAccess>()
				.Where(indexer => indexer.Parameters.Length == parameters.Length &&
				value is null ? indexer.Value is null : value!.Matches(indexer.Value) &&
				!parameters.Where((parameter, i) => parameter is null
					? indexer.Parameters[1] is null
					: !parameter.Matches(indexer.Parameters[1])).Any())
				.Cast<IInteraction>()
				.ToArray(),
        $"set indexer {string.Join(", ", parameters.Select(x => x?.ToString() ?? "null"))} to value {(value?.ToString() ?? "null")}");

	/// <summary>
	///     A proxy implementation of <see cref="IMockSet{TMock}" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockSetIndexer<TMock> inner, IMockVerify<TMock> verify)
		: MockSetIndexer<T, TMock>(verify), IMockSetIndexer<TMock>
	{
		/// <inheritdoc cref="IMockSetIndexer{TMock}.Set(With.Parameter?, With.Parameter?[])" />
		VerificationResult<TMock> IMockSetIndexer<TMock>.Set(With.Parameter? value, params With.Parameter?[] parameters)
			=> inner.Set(value, parameters);
	}

	/// <summary>
	///     Check which protected indexers were set on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockVerify<TMock> verify)
		: MockSetIndexer<T, TMock>(verify), IMockSetIndexer<TMock>
	{
	}
}
