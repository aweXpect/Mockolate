using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate.Verify;

/// <summary>
///     Verifications on a property of type <typeparamref name="TParameter" />.
/// </summary>
#if RELEASE
[DebuggerNonUserCode]
#endif
public class VerificationPropertyResult<TSubject, TParameter>
{
	private readonly MockRegistry _mockRegistry;
	private readonly string _propertyName;
	private readonly TSubject _subject;

	/// <inheritdoc cref="VerificationPropertyResult{TSubject, TParameter}" />
	public VerificationPropertyResult(TSubject subject, MockRegistry mockRegistry, string propertyName)
	{
		_subject = subject;
		_mockRegistry = mockRegistry;
		_propertyName = propertyName;
	}

	/// <summary>
	///     Verifies the property read access on the mock.
	/// </summary>
	public VerificationResult<TSubject> Got()
		=> _mockRegistry.Property(_subject, _propertyName);

	/// <summary>
	///     Verifies the property write access on the mock with the given <paramref name="value" />.
	/// </summary>
	public VerificationResult<TSubject> Set(IParameter<TParameter> value)
		=> _mockRegistry.Property(_subject, _propertyName, (IParameter)value);

	/// <summary>
	///     Verifies the property write access on the mock with the given <paramref name="value" />.
	/// </summary>
	[OverloadResolutionPriority(1)]
	public VerificationResult<TSubject> Set(TParameter value, [CallerArgumentExpression(nameof(value))] string doNotPopulateThisValue = "")
		=> _mockRegistry.Property(_subject, _propertyName, (IParameter)It.Is(value, doNotPopulateThisValue));
}
