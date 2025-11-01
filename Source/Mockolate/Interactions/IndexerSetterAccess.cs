using System.Diagnostics;
using System.Linq;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer setter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class IndexerSetterAccess(int index, object?[] parameters, object? value) : IInteraction
{
	/// <summary>
	///     The parameters of the indexer.
	/// </summary>
	public object?[] Parameters { get; } = parameters;

	/// <summary>
	///     The value the indexer was being set to.
	/// </summary>
	public object? Value { get; } = value;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index { get; } = index;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"[{Index}] set indexer [{string.Join(", ", Parameters.Select(p => p?.ToString() ?? "null"))}] to {Value ?? "null"}";
}
