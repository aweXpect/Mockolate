using System.Reflection;

namespace Mockolate.Events;

/// <summary>
///     Raise protected events on the mock for <typeparamref name="T" />.
/// </summary>
public class ProtectedMockRaises<T>(IMockRaises inner)
	: MockRaises<T>(inner.Setup, inner.Interactions), IMockRaises
{
	/// <inheritdoc cref="IMockRaises.Raise(string, object?[])" />
	void IMockRaises.Raise(string eventName, params object?[] parameters)
		=> inner.Raise(eventName, parameters);

	/// <inheritdoc cref="IMockRaises.AddEvent(string, object?, MethodInfo?)" />
	void IMockRaises.AddEvent(string name, object? target, MethodInfo? method)
		=> inner.AddEvent(name, target, method);

	/// <inheritdoc cref="IMockRaises.RemoveEvent(string, object?, MethodInfo?)" />
	void IMockRaises.RemoveEvent(string name, object? target, MethodInfo? method)
		=> inner.RemoveEvent(name, target, method);
}
