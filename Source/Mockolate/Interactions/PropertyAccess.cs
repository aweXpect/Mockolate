using System;
using System.Diagnostics;

namespace Mockolate.Interactions;

/// <summary>
///     An access of a property.
/// </summary>
[DebuggerDisplay("{ToString()}")]
[DebuggerNonUserCode]
public abstract class PropertyAccess(string propertyName) : IInteraction, ISettableInteraction
{
	private int? _index;
	/// <summary>
	///     The name of the property.
	/// </summary>
	public string Name { get; } = propertyName;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index => _index.GetValueOrDefault();

	void ISettableInteraction.SetIndex(int value) => _index ??= value;
}
