using System;
using Mockerade.Setup;

namespace Mockerade;

/// <summary>
///     Allows registration of method calls and property accesses on a mock.
/// </summary>
public interface IMock
{
	/// <summary>
	///     Executes the method with <paramref name="methodName"/> and the matching <paramref name="parameters"/> and gets the setup return value.
	/// </summary>
	MethodSetupResult<TResult> Execute<TResult>(string methodName, params object?[] parameters);

	/// <summary>
	///     Executes the method with <paramref name="methodName"/> and the matching <paramref name="parameters"/> returning <see langword="void" />.
	/// </summary>
	MethodSetupResult Execute(string methodName, params object?[] parameters);

	/// <summary>
	///     Accesses the getter of the property with <paramref name="propertyName"/>.
	/// </summary>
	TResult Get<TResult>(string propertyName);

	/// <summary>
	///     Accesses the setter of the property with <paramref name="propertyName"/> and the matching <paramref name="value"/>.
	/// </summary>
	void Set(string propertyName, object? value);

	/// <summary>
	///     Gets the interface used to raise events on the mock object.
	/// </summary>
	IMockRaises Raises { get; }
}
