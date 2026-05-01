using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate.Verify;

/// <summary>
///     Verifications on a property of type <typeparamref name="TParameter" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class VerificationPropertyResult<TSubject, TParameter>
{
	private const int NoMemberId = -1;
	private readonly int _getMemberId;

	private readonly MockRegistry _mockRegistry;
	private readonly string _propertyName;
	private readonly int _setMemberId;
	private readonly TSubject _subject;

	/// <inheritdoc cref="VerificationPropertyResult{TSubject, TParameter}" />
	public VerificationPropertyResult(TSubject subject, MockRegistry mockRegistry, string propertyName)
		: this(subject, mockRegistry, NoMemberId, NoMemberId, propertyName)
	{
	}

	/// <summary>
	///     Member-id-keyed constructor used by generated mocks to enable per-member fast Verify walks.
	/// </summary>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="mockRegistry">The mock registry holding the recorded interactions.</param>
	/// <param name="getMemberId">Member id of the property getter, or <c>-1</c> when unknown.</param>
	/// <param name="setMemberId">Member id of the property setter, or <c>-1</c> when unknown.</param>
	/// <param name="propertyName">The simple property name.</param>
	public VerificationPropertyResult(TSubject subject, MockRegistry mockRegistry,
		int getMemberId, int setMemberId, string propertyName)
	{
		_subject = subject;
		_mockRegistry = mockRegistry;
		_getMemberId = getMemberId;
		_setMemberId = setMemberId;
		_propertyName = propertyName;
	}

	/// <summary>
	///     Verifies the property read access on the mock.
	/// </summary>
	public VerificationResult<TSubject> Got()
		=> _mockRegistry.VerifyPropertyTyped(_subject, _getMemberId, _propertyName);

	/// <summary>
	///     Verifies the property write access on the mock with the given <paramref name="value" />.
	/// </summary>
	public VerificationResult<TSubject> Set(IParameter<TParameter> value)
		=> _mockRegistry.VerifyPropertyTyped(_subject, _setMemberId, _propertyName, value.AsParameterMatch());

	/// <summary>
	///     Verifies the property write access on the mock with the given <paramref name="value" />.
	/// </summary>
	[OverloadResolutionPriority(1)]
	public VerificationResult<TSubject> Set(TParameter value,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue = "")
		=> _mockRegistry.VerifyPropertyTyped(_subject, _setMemberId, _propertyName,
			It.Is(value, doNotPopulateThisValue).AsParameterMatch());
}
