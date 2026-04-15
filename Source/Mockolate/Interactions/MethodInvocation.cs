using System;
using System.Diagnostics;
using System.Linq;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate.Interactions;

/// <summary>
///     An invocation of a method.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class MethodInvocation(string name, INamedParameterValue[] parameters) : IInteraction
{
	/// <summary>
	///     The name of the method.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	///     The named parameters of the method.
	/// </summary>
	public INamedParameterValue[] Parameters { get; } = parameters;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"invoke method {Name.SubstringAfterLast('.')}({string.Join(", ", Parameters.Select(p => p.ToString()))})";
}
