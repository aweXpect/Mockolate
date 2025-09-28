using System.Collections.Generic;
using System.Reflection;

namespace Mockerade.Setup;

/// <summary>
///     Allows registration of <see cref="MethodSetup" /> in the mock.
/// </summary>
public interface IMockSetup
{
	/// <summary>
	///     Gets the underlying <see cref="IMock" /> instance.
	/// </summary>
	IMock Mock { get; }

	/// <summary>
	///     Registers the <paramref name="methodSetup" /> in the mock.
	/// </summary>
	void RegisterMethod(MethodSetup methodSetup);

	/// <summary>
	///     Registers the <paramref name="propertySetup" /> in the mock.
	/// </summary>
	void RegisterProperty(string propertyName, PropertySetup propertySetup);

	/// <summary>
	/// Gets all event handlers registered for the specified <paramref name="eventName"/>.
	/// </summary>
	/// <param name="eventName"></param>
	/// <returns></returns>
	IEnumerable<(object?, MethodInfo)> GetEventHandlers(string eventName);

	/// <summary>
	/// Registers an event handler <paramref name="method"/> on <paramref name="target"/> for the specified <paramref name="eventName"/>.
	/// </summary>
	void AddEvent(string eventName, object? target, MethodInfo method);

	/// <summary>
	/// Removes a previously registered event handler <paramref name="method"/> on <paramref name="target"/> for the specified <paramref name="eventName"/>.
	/// </summary>
	void RemoveEvent(string eventName, object? target, MethodInfo method);
}
