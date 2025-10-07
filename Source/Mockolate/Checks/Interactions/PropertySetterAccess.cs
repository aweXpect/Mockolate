namespace Mockolate.Checks.Interactions;

/// <summary>
///     An access of a property setter.
/// </summary>
public class PropertySetterAccess(int index, string propertyName, object? value) : IInteraction
{
	/// <summary>
	///     The name of the property.
	/// </summary>
	public string Name { get; } = propertyName;

	/// <summary>
	///     The value the property was being set to.
	/// </summary>
	public object? Value { get; } = value;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index { get; } = index;
}
