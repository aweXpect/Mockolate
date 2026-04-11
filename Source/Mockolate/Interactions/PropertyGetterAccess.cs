using System.Diagnostics;
using Mockolate.Internals;

namespace Mockolate.Interactions;

/// <summary>
///     An access of a property getter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class PropertyGetterAccess(string propertyName) : PropertyAccess(propertyName)
{
	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => $"[{Index}] get property {Name.SubstringAfterLast('.')}";
}
