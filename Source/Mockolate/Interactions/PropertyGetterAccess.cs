using System.Diagnostics;

namespace Mockolate.Interactions;

/// <summary>
///     An access of a property getter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class PropertyGetterAccess(int index, string propertyName) : PropertyAccess(index, propertyName)
{
	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => $"[{Index}] get property {Name}";
}
