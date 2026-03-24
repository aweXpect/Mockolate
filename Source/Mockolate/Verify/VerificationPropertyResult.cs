using System.Diagnostics;
using Mockolate.Parameters;

namespace Mockolate.Verify;

/// <summary>
///     Verifications on a property of type <typeparamref name="TParameter" />.
/// </summary>
[DebuggerNonUserCode]
public class VerificationPropertyResult<TSubject, TParameter>
{
	private readonly string _propertyName;
	private readonly MockRegistry _mockRegistry;
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
	public VerificationResult<TSubject> Set(IParameter<TParameter>? value)
		=> _mockRegistry.Property(_subject, _propertyName, (IParameter)(value ?? It.IsNull<TParameter>()));
}
