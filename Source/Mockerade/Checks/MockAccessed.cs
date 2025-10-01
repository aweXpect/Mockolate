using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mockerade.Setup;

namespace Mockerade.Checks;

/// <summary>
///     Check which properties were accessed on the mocked instance for <typeparamref name="T"/>.
/// </summary>
public class MockAccessed<T>(MockInvocations invocations) : IMockAccessed
{
	/// <summary>
	///     A proxy implementation of <see cref="IMockAccessed"/> that forwards all calls to the provided <paramref name="inner"/> instance.
	/// </summary>
	public class Proxy(IMockAccessed inner, MockInvocations invocations) : MockAccessed<T>(invocations), IMockAccessed
	{
		/// <inheritdoc cref="IMockAccessed.PropertyGetter(string)" />
		Invocation[] IMockAccessed.PropertyGetter(string propertyName)
			=> inner.PropertyGetter(propertyName);

		/// <inheritdoc cref="IMockAccessed.PropertySetter(string, With.Parameter)" />
		Invocation[] IMockAccessed.PropertySetter(string propertyName, With.Parameter value)
			=> inner.PropertySetter(propertyName, value);
	}

	/// <summary>
	///     Check which protected properties were accessed on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public class Protected(IMockAccessed inner, MockInvocations invocations) : MockAccessed<T>(invocations), IMockAccessed
	{
		/// <inheritdoc cref="IMockAccessed.PropertyGetter(string)" />
		Invocation[] IMockAccessed.PropertyGetter(string propertyName)
			=> inner.PropertyGetter(propertyName);

		/// <inheritdoc cref="IMockAccessed.PropertySetter(string, With.Parameter)" />
		Invocation[] IMockAccessed.PropertySetter(string propertyName, With.Parameter value)
			=> inner.PropertySetter(propertyName, value);
	}

	/// <inheritdoc cref="IMockAccessed.PropertyGetter(string)"/>
	Invocation[] IMockAccessed.PropertyGetter(string propertyName)
	{
		return invocations.Invocations
			.OfType<PropertyGetterInvocation>()
			.Where(property => property.Name.Equals(propertyName))
			.ToArray();
	}

	/// <inheritdoc cref="IMockAccessed.PropertySetter(string, With.Parameter)"/>
	Invocation[] IMockAccessed.PropertySetter(string propertyName, With.Parameter value)
	{
		return invocations.Invocations
			.OfType<PropertySetterInvocation>()
			.Where(property => property.Name.Equals(propertyName) && value.Matches(property.Value))
			.ToArray();
	}
}
