using System.Reflection;

namespace Mockolate.Checks.Interactions;

/// <summary>
///     An invocation of a method.
/// </summary>
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
}
