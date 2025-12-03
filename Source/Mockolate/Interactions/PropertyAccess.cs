using System.Diagnostics;

namespace Mockolate.Interactions;

/// <summary>
///     An access of a property.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public abstract class PropertyAccess(int index, string propertyName) : IInteraction
{
	/// <summary>
	///     The name of the property.
	/// </summary>
	public string Name { get; } = propertyName;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index { get; } = index;
}
