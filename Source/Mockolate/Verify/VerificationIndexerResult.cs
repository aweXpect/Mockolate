using System;
using System.Runtime.CompilerServices;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Verify;

/// <summary>
///     Verifications on an indexer of type <typeparamref name="TParameter" />.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public class VerificationIndexerResult<TSubject, TParameter>
{
	private const int NoMemberId = -1;

	private readonly MockRegistry _mockRegistry;
	private readonly Func<IInteraction, bool> _gotPredicate;
	private readonly Func<IInteraction, IParameterMatch<TParameter>, bool> _setPredicate;
	private readonly Func<string> _parametersDescription;
	private readonly TSubject _subject;
	private readonly int _getMemberId;
	private readonly int _setMemberId;

	/// <inheritdoc cref="VerificationIndexerResult{TSubject, TParameter}" />
	public VerificationIndexerResult(TSubject subject, MockRegistry mockRegistry,
		Func<IInteraction, bool> gotPredicate,
		Func<IInteraction, IParameterMatch<TParameter>, bool> setPredicate,
		Func<string> parametersDescription)
		: this(subject, mockRegistry, NoMemberId, NoMemberId, gotPredicate, setPredicate, parametersDescription)
	{
	}

	/// <summary>
	///     Member-id-keyed constructor used by generated mocks to enable per-member fast Verify walks.
	/// </summary>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="mockRegistry">The mock registry holding the recorded interactions.</param>
	/// <param name="getMemberId">Member id of the indexer getter, or <c>-1</c> when unknown.</param>
	/// <param name="setMemberId">Member id of the indexer setter, or <c>-1</c> when unknown.</param>
	/// <param name="gotPredicate">Predicate evaluated against each recorded indexer-getter interaction.</param>
	/// <param name="setPredicate">Predicate evaluated against each recorded indexer-setter interaction and the value matcher.</param>
	/// <param name="parametersDescription">Factory producing the indexer-argument description used in failure messages.</param>
	public VerificationIndexerResult(TSubject subject, MockRegistry mockRegistry,
		int getMemberId, int setMemberId,
		Func<IInteraction, bool> gotPredicate,
		Func<IInteraction, IParameterMatch<TParameter>, bool> setPredicate,
		Func<string> parametersDescription)
	{
		_subject = subject;
		_mockRegistry = mockRegistry;
		_getMemberId = getMemberId;
		_setMemberId = setMemberId;
		_gotPredicate = gotPredicate;
		_setPredicate = setPredicate;
		_parametersDescription = parametersDescription;
	}

	/// <summary>
	///     Verifies the indexer read access on the mock.
	/// </summary>
	public VerificationResult<TSubject> Got()
		=> _getMemberId >= 0
			? _mockRegistry.IndexerGot(_subject, _getMemberId, _gotPredicate, _parametersDescription)
			: _mockRegistry.IndexerGot(_subject, _gotPredicate, _parametersDescription);

	/// <summary>
	///     Verifies the indexer write access on the mock with the given <paramref name="value" />.
	/// </summary>
	public VerificationResult<TSubject> Set(IParameter<TParameter> value)
		=> _setMemberId >= 0
			? _mockRegistry.IndexerSet(_subject, _setMemberId, _setPredicate,
				(IParameterMatch<TParameter>)value, _parametersDescription)
			: _mockRegistry.IndexerSet(_subject, _setPredicate,
				(IParameterMatch<TParameter>)value, _parametersDescription);

	/// <summary>
	///     Verifies the indexer write access on the mock with the given <paramref name="value" />.
	/// </summary>
	[OverloadResolutionPriority(1)]
	public VerificationResult<TSubject> Set(TParameter value,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue = "")
		=> _setMemberId >= 0
			? _mockRegistry.IndexerSet(_subject, _setMemberId, _setPredicate,
				(IParameterMatch<TParameter>)It.Is(value, doNotPopulateThisValue), _parametersDescription)
			: _mockRegistry.IndexerSet(_subject, _setPredicate,
				(IParameterMatch<TParameter>)It.Is(value, doNotPopulateThisValue), _parametersDescription);
}
