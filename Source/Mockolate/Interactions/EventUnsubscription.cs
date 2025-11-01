using System.Diagnostics;
using System.Reflection;

namespace Mockolate.Interactions;

/// <summary>
///     An unsubscription from an event.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class EventUnsubscription(int index, string name, object? target, MethodInfo method) : IInteraction
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

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index { get; } = index;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => $"[{Index}] unsubscribe from event {Name}";
}
