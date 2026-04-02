using System;
using System.Diagnostics;
using System.Reflection;
using Mockolate.Internals;

namespace Mockolate.Interactions;

/// <summary>
///     An unsubscription from an event.
/// </summary>
[DebuggerDisplay("{ToString()}")]
[DebuggerNonUserCode]
public class EventUnsubscription(string name, object? target, MethodInfo method) : IInteraction, ISettableInteraction
{
	private int? _index;
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
	public int Index => _index.GetValueOrDefault();

	void ISettableInteraction.SetIndex(int value) => _index ??= value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => $"[{Index}] unsubscribe from event {Name.SubstringAfterLast('.')}";
}
