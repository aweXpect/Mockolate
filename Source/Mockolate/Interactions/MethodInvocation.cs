using System.Diagnostics;
using System.Linq;

namespace Mockolate.Interactions;

/// <summary>
///     An invocation of a method.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class MethodInvocation(int index, string name, object?[] parameters) : IInteraction
{
	/// <summary>
	///     The name of the method.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	///     The parameters of the method.
	/// </summary>
	public object?[] Parameters { get; } = parameters;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index { get; } = index;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"[{Index}] invoke method {Name}({string.Join(", ", Parameters.Select(p => p?.ToString() ?? "null"))})";
}
