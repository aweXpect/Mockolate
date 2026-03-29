using System;
using System.Diagnostics;

namespace Mockolate.Interactions;

/// <summary>
///     An access of a property.
/// </summary>
[DebuggerDisplay("{ToString()}")]
[DebuggerNonUserCode]
public abstract class PropertyAccess(string propertyName) : IInteraction
{
	/// <summary>
	///     The name of the property.
	/// </summary>
	public string Name { get; } = propertyName;

	/// <inheritdoc cref="IInteraction.Index" />
	public int? Index
	{
		get;
		set => field ??= value;
	}
}
