using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which properties got read on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockGot<T, TMock>(MockVerify<T, TMock> verify) : IMockGot<MockVerify<T, TMock>>
{
	internal MockVerify<T, TMock> Verify { get; } = verify;

	/// <inheritdoc cref="IMockGot{TMock}.Property(string)" />
	VerificationResult<MockVerify<T, TMock>> IMockGot<MockVerify<T, TMock>>.Property(string propertyName)
	{
		MockInteractions interactions = ((IMockVerify<TMock>)Verify).Interactions;
		return new(Verify, interactions,
			interactions.Interactions
				.OfType<PropertyGetterAccess>()
				.Where(property => property.Name.Equals(propertyName))
				.Cast<IInteraction>()
				.ToArray(),
		$"got property {propertyName.SubstringAfterLast('.')}");
	}
}
