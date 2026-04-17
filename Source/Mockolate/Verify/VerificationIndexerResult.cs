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
	private readonly MockRegistry _mockRegistry;
	private readonly Func<IInteraction, bool> _gotPredicate;
	private readonly Func<IInteraction, IParameterMatch<TParameter>, bool> _setPredicate;
	private readonly Func<string> _parametersDescription;
	private readonly TSubject _subject;

	/// <inheritdoc cref="VerificationIndexerResult{TSubject, TParameter}" />
	public VerificationIndexerResult(TSubject subject, MockRegistry mockRegistry,
		Func<IInteraction, bool> gotPredicate,
		Func<IInteraction, IParameterMatch<TParameter>, bool> setPredicate,
		Func<string> parametersDescription)
	{
		_subject = subject;
		_mockRegistry = mockRegistry;
		_gotPredicate = gotPredicate;
		_setPredicate = setPredicate;
		_parametersDescription = parametersDescription;
	}

	/// <summary>
	///     Verifies the indexer read access on the mock.
	/// </summary>
	public VerificationResult<TSubject> Got()
		=> _mockRegistry.IndexerGot(_subject, _gotPredicate, _parametersDescription);

	/// <summary>
	///     Verifies the indexer write access on the mock with the given <paramref name="value" />.
	/// </summary>
	public VerificationResult<TSubject> Set(IParameter<TParameter> value)
		=> _mockRegistry.IndexerSet(_subject, _setPredicate,
			(IParameterMatch<TParameter>)value, _parametersDescription);

	/// <summary>
	///     Verifies the indexer write access on the mock with the given <paramref name="value" />.
	/// </summary>
	[OverloadResolutionPriority(1)]
	public VerificationResult<TSubject> Set(TParameter value,
		[CallerArgumentExpression(nameof(value))] string doNotPopulateThisValue = "")
		=> _mockRegistry.IndexerSet(_subject, _setPredicate,
			(IParameterMatch<TParameter>)It.Is(value, doNotPopulateThisValue), _parametersDescription);
}
