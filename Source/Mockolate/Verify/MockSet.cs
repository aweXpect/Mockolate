using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which properties were set on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockSet<T, TMock>(IMockVerify<TMock> verify) : IMockSet<TMock>
{
	/// <inheritdoc cref="IMockSet{TMock}.Property(string, With.Parameter)" />
	VerificationResult<TMock> IMockSet<TMock>.Property(string propertyName, With.Parameter value)
		=> new(verify.Mock, verify.Interactions,
			verify.Interactions.Interactions
				.OfType<PropertySetterAccess>()
				.Where(property => property.Name.Equals(propertyName) && value.Matches(property.Value))
				.Cast<IInteraction>()
				.ToArray(),
        $"set property {propertyName.SubstringAfterLast('.')} to value {value}");

	/// <summary>
	///     Check which protected properties were set on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockVerify<TMock> verify) : MockSet<T, TMock>(verify)
	{
	}
}
