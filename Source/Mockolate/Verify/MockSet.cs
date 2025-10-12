using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which properties were set on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockSet<T, TMock>(MockVerify<T, TMock> verify) : IMockSet<MockVerify<T, TMock>>
{
	/// <inheritdoc cref="IMockSet{TMock}.Property(string, With.Parameter)" />
	VerificationResult<MockVerify<T, TMock>> IMockSet<MockVerify<T, TMock>>.Property(string propertyName, With.Parameter value)
	{
		MockInteractions interactions = ((IMockVerify<TMock>)verify).Interactions;
		return new(verify, interactions,
			interactions.Interactions
				.OfType<PropertySetterAccess>()
				.Where(property => property.Name.Equals(propertyName) && value.Matches(property.Value))
				.Cast<IInteraction>()
				.ToArray(),
		$"set property {propertyName.SubstringAfterLast('.')} to value {value}");
	}

	/// <summary>
	///     Check which protected properties were set on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(MockVerify<T, TMock> verify) : MockSet<T, TMock>(verify)
	{
	}
}
