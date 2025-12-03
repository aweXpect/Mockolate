using System.Diagnostics;

namespace Mockolate.Interactions;

/// <summary>
///     An access of a property setter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class PropertySetterAccess(int index, string propertyName, object? value) : PropertyAccess(index, propertyName)
{
	/// <summary>
	///     The value the property was being set to.
	/// </summary>
	public object? Value { get; } = value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => $"[{Index}] set property {Name} to {Value ?? "null"}";
}
