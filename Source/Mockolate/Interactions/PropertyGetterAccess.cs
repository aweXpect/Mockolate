using System.Diagnostics;

namespace Mockolate.Interactions;

/// <summary>
///     An access of a property getter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class PropertyGetterAccess(int index, string propertyName) : IInteraction
{
	/// <summary>
	///     The name of the property.
	/// </summary>
	public string Name { get; } = propertyName;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index { get; } = index;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
	{
		return $"[{Index}] get property {Name}";
	}
}
