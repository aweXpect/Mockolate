using System.Collections.Generic;
using System.Linq;

namespace Mockerade.Invocations;

/// <summary>
///     The invocations of the <see cref="Mock{T}" />
/// </summary>
public class MockInvocations<T> : IMockInvocations
{
	/// <summary>
	/// Indicates whether (at least) one invocation was already triggered.
	/// </summary>
	internal bool IsAlreadyInvoked => _invocations.Count > 0;

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

	/// <inheritdoc cref="IMockInvocations.Method(string, With.Parameter[])"/>
	Invocation[] IMockInvocations.Method(string methodName, params With.Parameter[] parameters)
	{
		return _invocations
			.OfType<MethodInvocation>()
			.Where(method =>
				method.Name.Equals(methodName) &&
				method.Parameters.Length == parameters.Length &&
				!parameters.Where((parameter, i) => !parameter.Matches(method.Parameters[i])).Any())
			.ToArray();
	}

	/// <inheritdoc cref="IMockInvocations.PropertyGetter(string)"/>
	Invocation[] IMockInvocations.PropertyGetter(string propertyName)
	{
		return _invocations
			.OfType<PropertyGetterInvocation>()
			.Where(property => property.Name.Equals(propertyName))
			.ToArray();
	}

	/// <inheritdoc cref="IMockInvocations.PropertySetter(string, With.Parameter)"/>
	Invocation[] IMockInvocations.PropertySetter(string propertyName, With.Parameter value)
	{
		return _invocations
			.OfType<PropertySetterInvocation>()
			.Where(property => property.Name.Equals(propertyName) && value.Matches(property.Value))
			.ToArray();
	}
}
