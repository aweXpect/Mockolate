namespace Mockolate.Checks;

/// <summary>
///     An invocation of a property getter.
/// </summary>
public class PropertyGetterInvocation(string propertyName) : IInvocation
{
	/// <summary>
	///     The name of the property.
	/// </summary>
	public string Name { get; } = propertyName;
}
