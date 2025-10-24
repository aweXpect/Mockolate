using System.Collections.Generic;
using System.Reflection;

namespace Mockolate.Setup;

/// <summary>
///     Sets up the protected elements of the mock for <typeparamref name="T" />.
/// </summary>
public class ProtectedMockSetup<T>(IMockSetup inner) : MockSetup<T>(inner.Mock), IMockSetup
{
	/// <inheritdoc cref="IMockSetup.Mock" />
	public IMock Mock
		=> inner.Mock;

	/// <inheritdoc cref="IMockSetup.RegisterIndexer(IndexerSetup)" />
	void IMockSetup.RegisterIndexer(IndexerSetup indexerSetup)
		=> inner.RegisterIndexer(indexerSetup);

	/// <inheritdoc cref="IMockSetup.RegisterMethod(MethodSetup)" />
	void IMockSetup.RegisterMethod(MethodSetup methodSetup)
		=> inner.RegisterMethod(methodSetup);

	/// <inheritdoc cref="IMockSetup.RegisterProperty(string, PropertySetup)" />
	void IMockSetup.RegisterProperty(string propertyName, PropertySetup propertySetup)
		=> inner.RegisterProperty(propertyName, propertySetup);

	/// <inheritdoc cref="IMockSetup.GetEventHandlers(string)" />
	IEnumerable<(object?, MethodInfo)> IMockSetup.GetEventHandlers(string eventName)
		=> inner.GetEventHandlers(eventName);

	/// <inheritdoc cref="IMockSetup.AddEvent(string, object?, MethodInfo)" />
	void IMockSetup.AddEvent(string eventName, object? target, MethodInfo method)
		=> inner.AddEvent(eventName, target, method);

	/// <inheritdoc cref="IMockSetup.RemoveEvent(string, object?, MethodInfo)" />
	void IMockSetup.RemoveEvent(string eventName, object? target, MethodInfo method)
		=> inner.RemoveEvent(eventName, target, method);

	/// <inheritdoc cref="IMockSetup.SetIndexerValue{TValue}(object?[], TValue)" />
	public void SetIndexerValue<TValue>(object?[] parameters, TValue value)
		=> inner.SetIndexerValue(parameters, value);
}
