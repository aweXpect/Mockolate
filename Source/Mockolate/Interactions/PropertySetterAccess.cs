using System.Diagnostics;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate.Interactions;

/// <summary>
///     An access of a property setter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if RELEASE
[DebuggerNonUserCode]
#endif
public class PropertySetterAccess(string propertyName, INamedParameterValue value) : PropertyAccess(propertyName)
{
	/// <summary>
	///     The value the property was being set to.
	/// </summary>
	public INamedParameterValue Value { get; } = value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => $"[{Index}] set property {Name.SubstringAfterLast('.')} to {Value}";
}
