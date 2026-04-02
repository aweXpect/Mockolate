using System.Diagnostics;
using Mockolate.Internals;

namespace Mockolate.Interactions;

/// <summary>
///     An access of a property setter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
[DebuggerNonUserCode]
public class PropertySetterAccess(string propertyName, object? value) : PropertyAccess(propertyName)
{
	/// <summary>
	///     The value the property was being set to.
	/// </summary>
	public object? Value { get; } = value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => $"[{Index}] set property {Name.SubstringAfterLast('.')} to {Value ?? "null"}";
}
