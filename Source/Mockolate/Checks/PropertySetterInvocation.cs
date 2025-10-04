namespace Mockolate.Checks;

/// <summary>
///     An invocation of a property setter.
/// </summary>
public class PropertySetterInvocation(string propertyName, object? value) : Invocation
{
	/// <summary>
	///     The name of the property.
	/// </summary>
	public string Name { get; } = propertyName;

	/// <summary>
	///     The value the property was being set to.
	/// </summary>
	public object? Value { get; } = value;
}
