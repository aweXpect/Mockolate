using System.Reflection;

namespace Mockolate.Checks;

/// <summary>
///     A subscription to an event.
/// </summary>
public class EventSubscription(string name, object? target, MethodInfo method) : Invocation
{
	/// <summary>
	///     The name of the event.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	///     The target of the event delegate.
	/// </summary>
	public object? Target { get; } = target;

	/// <summary>
	///     The method of the event delegate.
	/// </summary>
	public MethodInfo Method { get; } = method;
}
