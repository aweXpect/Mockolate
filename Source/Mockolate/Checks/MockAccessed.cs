using System.Linq;
using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Check which properties were accessed on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockAccessed<T, TMock>(Checks checks, TMock mock) : IMockAccessed<TMock>
{
	private readonly TMock _mock = mock;
	private readonly Checks _checks = checks;

	/// <inheritdoc cref="IMockAccessed{TMock}.PropertyGetter(string)" />
	CheckResult<TMock> IMockAccessed<TMock>.PropertyGetter(string propertyName) => new(_mock, _checks,
		_checks.Interactions
			.OfType<PropertyGetterAccess>()
			.Where(property => property.Name.Equals(propertyName))
			.ToArray());

	/// <inheritdoc cref="IMockAccessed{TMock}.PropertySetter(string, With.Parameter)" />
	CheckResult<TMock> IMockAccessed<TMock>.PropertySetter(string propertyName, With.Parameter value) => new(_mock, _checks,
		_checks.Interactions
			.OfType<PropertySetterAccess>()
			.Where(property => property.Name.Equals(propertyName) && value.Matches(property.Value))
			.ToArray());

	/// <summary>
	///     A proxy implementation of <see cref="IMockAccessed{TMock}" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockAccessed<TMock> inner, Checks checks, TMock mock)
		: MockAccessed<T, TMock>(checks, mock), IMockAccessed<TMock>
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
	public class Protected(IMockAccessed<TMock> inner, Checks checks, TMock mock)
		: MockAccessed<T, TMock>(checks, mock), IMockAccessed<TMock>
	{
		/// <inheritdoc cref="IMockAccessed{TMock}.PropertyGetter(string)" />
		CheckResult<TMock> IMockAccessed<TMock>.PropertyGetter(string propertyName)
			=> inner.PropertyGetter(propertyName);

		/// <inheritdoc cref="IMockAccessed{TMock}.PropertySetter(string, With.Parameter)" />
		CheckResult<TMock> IMockAccessed<TMock>.PropertySetter(string propertyName, With.Parameter value)
			=> inner.PropertySetter(propertyName, value);
	}
}
