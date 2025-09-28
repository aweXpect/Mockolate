using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mockerade.Setup;

namespace Mockerade.Checks;

/// <summary>
///     The invocations of the <see cref="Mock{T}" />
/// </summary>
public class MockChecks<T> : IMockChecks
{
	/// <summary>
	/// A proxy implementation of <see cref="IMockSetup"/> that forwards all calls to the provided <paramref name="inner"/> instance.
	/// </summary>
	public class Proxy(IMockChecks inner) : MockChecks<T>(), IMockChecks
	{
		/// <inheritdoc cref="IMockChecks.IsAlreadyInvoked" />
		bool IMockChecks.IsAlreadyInvoked
			=> inner.IsAlreadyInvoked;

		/// <inheritdoc cref="IMockChecks.Method(string, With.Parameter[])" />
		Invocation[] IMockChecks.Method(string methodName, params With.Parameter[] parameters)
			=> inner.Method(methodName, parameters);

		/// <inheritdoc cref="IMockChecks.PropertyGetter(string)" />
		Invocation[] IMockChecks.PropertyGetter(string propertyName)
			=> inner.PropertyGetter(propertyName);

		/// <inheritdoc cref="IMockChecks.PropertySetter(string, With.Parameter)" />
		Invocation[] IMockChecks.PropertySetter(string propertyName, With.Parameter value)
			=> inner.PropertySetter(propertyName, value);
	}

	/// <inheritdoc cref="IMockChecks.IsAlreadyInvoked" />
	bool IMockChecks.IsAlreadyInvoked => _invocations.Count > 0;

	private readonly List<Invocation> _invocations = [];

	/// <summary>
	///     The registered invocations of the mock.
	/// </summary>
	public IReadOnlyList<Invocation> Invocations => _invocations.AsReadOnly();

	internal Invocation RegisterInvocation(Invocation invocation)
	{
		_invocations.Add(invocation);
		return invocation;
	}

	/// <inheritdoc cref="IMockChecks.Method(string, With.Parameter[])"/>
	Invocation[] IMockChecks.Method(string methodName, params With.Parameter[] parameters)
	{
		return _invocations
			.OfType<MethodInvocation>()
			.Where(method =>
				method.Name.Equals(methodName) &&
				method.Parameters.Length == parameters.Length &&
				!parameters.Where((parameter, i) => !parameter.Matches(method.Parameters[i])).Any())
			.ToArray();
	}

	/// <inheritdoc cref="IMockChecks.PropertyGetter(string)"/>
	Invocation[] IMockChecks.PropertyGetter(string propertyName)
	{
		return _invocations
			.OfType<PropertyGetterInvocation>()
			.Where(property => property.Name.Equals(propertyName))
			.ToArray();
	}

	/// <inheritdoc cref="IMockChecks.PropertySetter(string, With.Parameter)"/>
	Invocation[] IMockChecks.PropertySetter(string propertyName, With.Parameter value)
	{
		return _invocations
			.OfType<PropertySetterInvocation>()
			.Where(property => property.Name.Equals(propertyName) && value.Matches(property.Value))
			.ToArray();
	}
}
