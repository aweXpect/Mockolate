namespace Mockolate.Checks.Interactions;

/// <summary>
///     An access of a property getter.
/// </summary>
public class PropertyGetterAccess(string propertyName) : IInteraction
{
	/// <summary>
	///     The name of the property.
	/// </summary>
	public string Name { get; } = propertyName;
}
