using System.Linq;
using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Check which properties were accessed on the mocked instance for <typeparamref name="T" />.
/// </summary>
public class MockAccessed<T>(MockChecks checks) : IMockAccessed
{
	/// <inheritdoc cref="IMockAccessed.PropertyGetter(string)" />
	CheckResult IMockAccessed.PropertyGetter(string propertyName) => new(checks,
		checks.Interactions
			.OfType<PropertyGetterAccess>()
			.Where(property => property.Name.Equals(propertyName))
			.ToArray());

	/// <inheritdoc cref="IMockAccessed.PropertySetter(string, With.Parameter)" />
	CheckResult IMockAccessed.PropertySetter(string propertyName, With.Parameter value) => new(checks,
		checks.Interactions
			.OfType<PropertySetterAccess>()
			.Where(property => property.Name.Equals(propertyName) && value.Matches(property.Value))
			.ToArray());

	/// <summary>
	///     A proxy implementation of <see cref="IMockAccessed" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockAccessed inner, MockChecks invocations) : MockAccessed<T>(invocations), IMockAccessed
	{
		/// <inheritdoc cref="IMockAccessed.PropertyGetter(string)" />
		CheckResult IMockAccessed.PropertyGetter(string propertyName)
			=> inner.PropertyGetter(propertyName);

		/// <inheritdoc cref="IMockAccessed.PropertySetter(string, With.Parameter)" />
		CheckResult IMockAccessed.PropertySetter(string propertyName, With.Parameter value)
			=> inner.PropertySetter(propertyName, value);
	}

	/// <summary>
	///     Check which protected properties were accessed on the mocked instance for <typeparamref name="T" />.
	/// </summary>
	public class Protected(IMockAccessed inner, MockChecks invocations)
		: MockAccessed<T>(invocations), IMockAccessed
	{
		/// <inheritdoc cref="IMockAccessed.PropertyGetter(string)" />
		CheckResult IMockAccessed.PropertyGetter(string propertyName)
			=> inner.PropertyGetter(propertyName);

		/// <inheritdoc cref="IMockAccessed.PropertySetter(string, With.Parameter)" />
		CheckResult IMockAccessed.PropertySetter(string propertyName, With.Parameter value)
			=> inner.PropertySetter(propertyName, value);
	}
}
