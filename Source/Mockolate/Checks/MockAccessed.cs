using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Checks;

/// <summary>
///     Check which properties were accessed on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockAccessed<T, TMock>(IMockVerify<TMock> verify) : IMockAccessed<TMock>
{
	/// <inheritdoc cref="IMockAccessed{TMock}.PropertyGetter(string)" />
	CheckResult<TMock> IMockAccessed<TMock>.PropertyGetter(string propertyName)
		=> new(verify.Mock, verify.Interactions,
			verify.Interactions.Interactions
				.OfType<PropertyGetterAccess>()
				.Where(property => property.Name.Equals(propertyName))
				.Cast<IInteraction>()
				.ToArray(),
        $"accessed getter of property {propertyName.SubstringAfterLast('.')}");

	/// <inheritdoc cref="IMockAccessed{TMock}.PropertySetter(string, With.Parameter)" />
	CheckResult<TMock> IMockAccessed<TMock>.PropertySetter(string propertyName, With.Parameter value)
		=> new(verify.Mock, verify.Interactions,
			verify.Interactions.Interactions
				.OfType<PropertySetterAccess>()
				.Where(property => property.Name.Equals(propertyName) && value.Matches(property.Value))
				.Cast<IInteraction>()
				.ToArray(),
        $"accessed setter of property {propertyName.SubstringAfterLast('.')} with value {value}");

	/// <summary>
	///     A proxy implementation of <see cref="IMockAccessed{TMock}" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockAccessed<TMock> inner, IMockVerify<TMock> verify)
		: MockAccessed<T, TMock>(verify), IMockAccessed<TMock>
	{
		/// <inheritdoc cref="IMockAccessed{TMock}.PropertyGetter(string)" />
		CheckResult<TMock> IMockAccessed<TMock>.PropertyGetter(string propertyName)
			=> inner.PropertyGetter(propertyName);

		/// <inheritdoc cref="IMockAccessed{TMock}.PropertySetter(string, With.Parameter)" />
		CheckResult<TMock> IMockAccessed<TMock>.PropertySetter(string propertyName, With.Parameter value)
			=> inner.PropertySetter(propertyName, value);
	}

	/// <summary>
	///     Check which protected properties were accessed on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockVerify<TMock> verify)
		: MockAccessed<T, TMock>(verify), IMockAccessed<TMock>
	{
	}
}
