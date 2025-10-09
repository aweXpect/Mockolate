using System.Diagnostics;
using System.Linq;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer getter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class IndexerGetterAccess(int index, object?[] parameters) : IInteraction
{
	/// <summary>
	///     The parameters of the indexer.
	/// </summary>
	public object?[] Parameters { get; } = parameters;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index { get; } = index;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
	{
		return $"[{Index}] get indexer [{string.Join(", ", Parameters.Select(p => p?.ToString() ?? "null"))}]";
	}
}
