using System.Diagnostics;
using System.Linq;
using Mockolate.Parameters;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer getter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class IndexerGetterAccess(int index, NamedParameterValue[] parameters) : IndexerAccess(index, parameters)
{
	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"[{Index}] get indexer [{string.Join(", ", Parameters.Select(p => p.Value?.ToString() ?? "null"))}]";
}
