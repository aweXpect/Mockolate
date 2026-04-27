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
	/// <summary>
	///     Sentinel value used by the typed-match subclasses to indicate that the legacy predicate-based
	///     <see cref="MockRegistry.IndexerGot{T}(T, int, Func{IInteraction, bool}, Func{string})" /> path is not used.
	/// </summary>
	private protected const int NoMemberId = -1;

	private readonly Func<IInteraction, bool>? _gotPredicate;
	private readonly Func<IInteraction, IParameterMatch<TParameter>, bool>? _setPredicate;

	/// <summary>The mock registry holding the recorded interactions.</summary>
	private protected readonly MockRegistry MockRegistry;
	/// <summary>Factory producing the indexer-argument description used in failure messages.</summary>
	private protected readonly Func<string> ParametersDescription;
	/// <summary>The verification facade the result is bound to.</summary>
	private protected readonly TSubject Subject;
	/// <summary>Member id of the indexer getter, or <c>-1</c> when unknown.</summary>
	private protected readonly int GetMemberId;
	/// <summary>Member id of the indexer setter, or <c>-1</c> when unknown.</summary>
	private protected readonly int SetMemberId;

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
		Subject = subject;
		MockRegistry = mockRegistry;
		GetMemberId = getMemberId;
		SetMemberId = setMemberId;
		_gotPredicate = gotPredicate;
		_setPredicate = setPredicate;
		ParametersDescription = parametersDescription;
	}

	/// <summary>
	///     Predicate-free constructor used by the typed-match subclasses, which dispatch through
	///     <see cref="MockRegistry.IndexerGotTyped{T, T1}" /> / <see cref="MockRegistry.IndexerSetTyped{T, T1, TValue}" />
	///     and never consult the base predicates.
	/// </summary>
	private protected VerificationIndexerResult(TSubject subject, MockRegistry mockRegistry,
		int getMemberId, int setMemberId,
		Func<string> parametersDescription)
	{
		Subject = subject;
		MockRegistry = mockRegistry;
		GetMemberId = getMemberId;
		SetMemberId = setMemberId;
		_gotPredicate = null;
		_setPredicate = null;
		ParametersDescription = parametersDescription;
	}

	/// <summary>
	///     Verifies the indexer read access on the mock.
	/// </summary>
	public virtual VerificationResult<TSubject> Got()
		=> MockRegistry.IndexerGot(Subject, GetMemberId, _gotPredicate!, ParametersDescription);

	/// <summary>
	///     Verifies the indexer write access on the mock with the given <paramref name="value" />.
	/// </summary>
	public virtual VerificationResult<TSubject> Set(IParameter<TParameter> value)
		=> MockRegistry.IndexerSet(Subject, SetMemberId, _setPredicate!,
			(IParameterMatch<TParameter>)value, ParametersDescription);

	/// <summary>
	///     Verifies the indexer write access on the mock with the given <paramref name="value" />.
	/// </summary>
	[OverloadResolutionPriority(1)]
	public virtual VerificationResult<TSubject> Set(TParameter value,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue = "")
		=> MockRegistry.IndexerSet(Subject, SetMemberId, _setPredicate!,
			(IParameterMatch<TParameter>)It.Is(value, doNotPopulateThisValue), ParametersDescription);
}

/// <summary>
///     Verifications on a 1-key indexer of type <typeparamref name="TParameter" />.
///     Bypasses the predicate-based <see cref="VerificationIndexerResult{TSubject, TParameter}" />
///     hot path and dispatches through the typed
///     <see cref="MockRegistry.IndexerGotTyped{T, T1}" /> /
///     <see cref="MockRegistry.IndexerSetTyped{T, T1, TValue}" /> overloads.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public sealed class VerificationIndexerResult<TSubject, T1, TParameter>
	: VerificationIndexerResult<TSubject, TParameter>
{
	private readonly IParameterMatch<T1> _match1;

	/// <inheritdoc cref="VerificationIndexerResult{TSubject, T1, TParameter}" />
	public VerificationIndexerResult(TSubject subject, MockRegistry mockRegistry,
		int getMemberId, int setMemberId,
		IParameterMatch<T1> match1,
		Func<string> parametersDescription)
		: base(subject, mockRegistry, getMemberId, setMemberId, parametersDescription)
	{
		_match1 = match1;
	}

	/// <inheritdoc />
	public override VerificationResult<TSubject> Got()
		=> MockRegistry.IndexerGotTyped<TSubject, T1>(Subject, GetMemberId, _match1, ParametersDescription);

	/// <inheritdoc />
	public override VerificationResult<TSubject> Set(IParameter<TParameter> value)
		=> MockRegistry.IndexerSetTyped<TSubject, T1, TParameter>(Subject, SetMemberId,
			_match1, (IParameterMatch<TParameter>)value, ParametersDescription);

	/// <inheritdoc />
	public override VerificationResult<TSubject> Set(TParameter value,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue = "")
		=> MockRegistry.IndexerSetTyped<TSubject, T1, TParameter>(Subject, SetMemberId,
			_match1, (IParameterMatch<TParameter>)It.Is(value, doNotPopulateThisValue), ParametersDescription);
}

/// <summary>
///     Verifications on a 2-key indexer of type <typeparamref name="TParameter" />. See
///     <see cref="VerificationIndexerResult{TSubject, T1, TParameter}" /> for rationale.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public sealed class VerificationIndexerResult<TSubject, T1, T2, TParameter>
	: VerificationIndexerResult<TSubject, TParameter>
{
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<T2> _match2;

	/// <inheritdoc cref="VerificationIndexerResult{TSubject, T1, T2, TParameter}" />
	public VerificationIndexerResult(TSubject subject, MockRegistry mockRegistry,
		int getMemberId, int setMemberId,
		IParameterMatch<T1> match1, IParameterMatch<T2> match2,
		Func<string> parametersDescription)
		: base(subject, mockRegistry, getMemberId, setMemberId, parametersDescription)
	{
		_match1 = match1;
		_match2 = match2;
	}

	/// <inheritdoc />
	public override VerificationResult<TSubject> Got()
		=> MockRegistry.IndexerGotTyped<TSubject, T1, T2>(Subject, GetMemberId, _match1, _match2, ParametersDescription);

	/// <inheritdoc />
	public override VerificationResult<TSubject> Set(IParameter<TParameter> value)
		=> MockRegistry.IndexerSetTyped<TSubject, T1, T2, TParameter>(Subject, SetMemberId,
			_match1, _match2, (IParameterMatch<TParameter>)value, ParametersDescription);

	/// <inheritdoc />
	public override VerificationResult<TSubject> Set(TParameter value,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue = "")
		=> MockRegistry.IndexerSetTyped<TSubject, T1, T2, TParameter>(Subject, SetMemberId,
			_match1, _match2, (IParameterMatch<TParameter>)It.Is(value, doNotPopulateThisValue), ParametersDescription);
}

/// <summary>
///     Verifications on a 3-key indexer of type <typeparamref name="TParameter" />. See
///     <see cref="VerificationIndexerResult{TSubject, T1, TParameter}" /> for rationale.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public sealed class VerificationIndexerResult<TSubject, T1, T2, T3, TParameter>
	: VerificationIndexerResult<TSubject, TParameter>
{
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<T2> _match2;
	private readonly IParameterMatch<T3> _match3;

	/// <inheritdoc cref="VerificationIndexerResult{TSubject, T1, T2, T3, TParameter}" />
	public VerificationIndexerResult(TSubject subject, MockRegistry mockRegistry,
		int getMemberId, int setMemberId,
		IParameterMatch<T1> match1, IParameterMatch<T2> match2, IParameterMatch<T3> match3,
		Func<string> parametersDescription)
		: base(subject, mockRegistry, getMemberId, setMemberId, parametersDescription)
	{
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
	}

	/// <inheritdoc />
	public override VerificationResult<TSubject> Got()
		=> MockRegistry.IndexerGotTyped<TSubject, T1, T2, T3>(Subject, GetMemberId,
			_match1, _match2, _match3, ParametersDescription);

	/// <inheritdoc />
	public override VerificationResult<TSubject> Set(IParameter<TParameter> value)
		=> MockRegistry.IndexerSetTyped<TSubject, T1, T2, T3, TParameter>(Subject, SetMemberId,
			_match1, _match2, _match3, (IParameterMatch<TParameter>)value, ParametersDescription);

	/// <inheritdoc />
	public override VerificationResult<TSubject> Set(TParameter value,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue = "")
		=> MockRegistry.IndexerSetTyped<TSubject, T1, T2, T3, TParameter>(Subject, SetMemberId,
			_match1, _match2, _match3, (IParameterMatch<TParameter>)It.Is(value, doNotPopulateThisValue),
			ParametersDescription);
}

/// <summary>
///     Verifications on a 4-key indexer of type <typeparamref name="TParameter" />. See
///     <see cref="VerificationIndexerResult{TSubject, T1, TParameter}" /> for rationale.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public sealed class VerificationIndexerResult<TSubject, T1, T2, T3, T4, TParameter>
	: VerificationIndexerResult<TSubject, TParameter>
{
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<T2> _match2;
	private readonly IParameterMatch<T3> _match3;
	private readonly IParameterMatch<T4> _match4;

	/// <inheritdoc cref="VerificationIndexerResult{TSubject, T1, T2, T3, T4, TParameter}" />
	public VerificationIndexerResult(TSubject subject, MockRegistry mockRegistry,
		int getMemberId, int setMemberId,
		IParameterMatch<T1> match1, IParameterMatch<T2> match2,
		IParameterMatch<T3> match3, IParameterMatch<T4> match4,
		Func<string> parametersDescription)
		: base(subject, mockRegistry, getMemberId, setMemberId, parametersDescription)
	{
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
		_match4 = match4;
	}

	/// <inheritdoc />
	public override VerificationResult<TSubject> Got()
		=> MockRegistry.IndexerGotTyped<TSubject, T1, T2, T3, T4>(Subject, GetMemberId,
			_match1, _match2, _match3, _match4, ParametersDescription);

	/// <inheritdoc />
	public override VerificationResult<TSubject> Set(IParameter<TParameter> value)
		=> MockRegistry.IndexerSetTyped<TSubject, T1, T2, T3, T4, TParameter>(Subject, SetMemberId,
			_match1, _match2, _match3, _match4, (IParameterMatch<TParameter>)value, ParametersDescription);

	/// <inheritdoc />
	public override VerificationResult<TSubject> Set(TParameter value,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue = "")
		=> MockRegistry.IndexerSetTyped<TSubject, T1, T2, T3, T4, TParameter>(Subject, SetMemberId,
			_match1, _match2, _match3, _match4,
			(IParameterMatch<TParameter>)It.Is(value, doNotPopulateThisValue), ParametersDescription);
}
