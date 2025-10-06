using System.Reflection;

namespace Mockolate.Checks.Interactions;

/// <summary>
///     An access of a property getter.
/// </summary>
public class PropertyGetterAccess(int index, string propertyName) : IInteraction
{
	/// <summary>
	///     The name of the property.
	/// </summary>
	public string Name { get; } = propertyName;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index { get; } = index;
}
