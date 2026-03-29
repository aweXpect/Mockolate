using System.Diagnostics;

namespace Mockolate.Interactions;

/// <summary>
///     An access of a property getter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
[DebuggerNonUserCode]
public class PropertyGetterAccess(string propertyName) : PropertyAccess(propertyName)
{
	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => $"[{Index}] get property {Name}";
}
