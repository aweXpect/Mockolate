using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate.Verify;

/// <summary>
///     Verifications on an indexer of type <typeparamref name="TParameter" />.
/// </summary>
#if RELEASE
[DebuggerNonUserCode]
#endif
public class VerificationIndexerResult<TSubject, TParameter>
{
	private readonly MockRegistry _mockRegistry;
	private readonly NamedParameter[] _parameters;
	private readonly TSubject _subject;

	/// <inheritdoc cref="VerificationIndexerResult{TSubject, TParameter}" />
	public VerificationIndexerResult(TSubject subject, MockRegistry mockRegistry, params NamedParameter[] parameters)
	{
		_subject = subject;
		_mockRegistry = mockRegistry;
		_parameters = parameters;
	}

	/// <summary>
	///     Verifies the indexer read access on the mock.
	/// </summary>
	public VerificationResult<TSubject> Got()
		=> _mockRegistry.Indexer(_subject, _parameters);

	/// <summary>
	///     Verifies the indexer write access on the mock with the given <paramref name="value" />.
	/// </summary>
	public VerificationResult<TSubject> Set(IParameter<TParameter> value)
		=> _mockRegistry.Indexer(_subject, (IParameter)value, _parameters);

	/// <summary>
	///     Verifies the indexer write access on the mock with the given <paramref name="value" />.
	/// </summary>
	[OverloadResolutionPriority(1)]
	public VerificationResult<TSubject> Set(TParameter value, [CallerArgumentExpression(nameof(value))] string doNotPopulateThisValue = "")
		=> _mockRegistry.Indexer(_subject, (IParameter)It.Is(value, doNotPopulateThisValue), _parameters);
}
