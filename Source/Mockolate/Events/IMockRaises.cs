using System;
using System.Reflection;

namespace Mockolate.Events;

/// <summary>
///     Provides methods for managing events on a mock object, including raising events and associating or dissociating event handlers.
/// </summary>
public interface IMockRaises
{
	/// <summary>
	///     Raises the event with <paramref name="eventName"/> and the given <paramref name="parameters"/>.
	/// </summary>
	void Raise(string eventName, params object?[] parameters);

	/// <summary>
	/// Associates the specified event <paramref name="method"/> on the <paramref name="target"/> with the event identified by the given <paramref name="name"/>.
	/// </summary>
	void AddEvent(string name, object? target, MethodInfo? method);

	/// <summary>
	/// Removes the specified event <paramref name="method"/> on the <paramref name="target"/> from the event identified by the given <paramref name="name"/>.
	/// </summary>
	void RemoveEvent(string name, object? target, MethodInfo? method);
}
