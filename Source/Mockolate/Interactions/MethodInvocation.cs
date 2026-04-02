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
[DebuggerNonUserCode]
public class MethodInvocation(string name, NamedParameterValue[] parameters) : IInteraction, ISettableInteraction
{
	private int? _index;
	/// <summary>
	///     The name of the method.
	/// </summary>
	public string Name { get; } = name;

	/// <summary>
	///     The named parameters of the method.
	/// </summary>
	public NamedParameterValue[] Parameters { get; } = parameters;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index => _index.GetValueOrDefault();

	void ISettableInteraction.SetIndex(int value) => _index ??= value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"[{Index}] invoke method {Name.SubstringAfterLast('.')}({string.Join(", ", Parameters.Select(p => p.Value?.ToString() ?? "null"))})";
}
