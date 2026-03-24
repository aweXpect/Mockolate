using System.Diagnostics;
using Mockolate.Parameters;

namespace Mockolate.Verify;

/// <summary>
///     Verifications on an indexer of type <typeparamref name="TParameter" />.
/// </summary>
[DebuggerNonUserCode]
public class VerificationIndexerResult<TSubject, TParameter>
{
	private readonly NamedParameter[] _parameters;
	private readonly MockRegistry _mockRegistry;
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
	public VerificationResult<TSubject> Set(IParameter<TParameter>? value)
		=> _mockRegistry.Indexer(_subject, (IParameter)(value ?? It.IsNull<TParameter>()), _parameters);
}
