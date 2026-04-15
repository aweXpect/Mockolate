using System.Diagnostics;

namespace Mockolate.Interactions;

/// <summary>
///     An access of a property.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public abstract class PropertyAccess(string propertyName) : IInteraction
{
	/// <summary>
	///     The name of the property.
	/// </summary>
	public string Name { get; } = propertyName;
}
