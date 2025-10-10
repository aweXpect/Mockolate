using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which properties got read on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockGot<T, TMock>(IMockVerify<TMock> verify) : IMockGot<TMock>
{
	/// <inheritdoc cref="IMockGot{TMock}.Property(string)" />
	VerificationResult<TMock> IMockGot<TMock>.Property(string propertyName)
		=> new(verify.Mock, verify.Interactions,
			verify.Interactions.Interactions
				.OfType<PropertyGetterAccess>()
				.Where(property => property.Name.Equals(propertyName))
				.Cast<IInteraction>()
				.ToArray(),
        $"got property {propertyName.SubstringAfterLast('.')}");

	/// <summary>
	///     Check which protected properties got read on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockVerify<TMock> verify)
		: MockGot<T, TMock>(verify), IMockGot<TMock>
	{
	}
}
