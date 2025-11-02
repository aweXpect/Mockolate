using System;
using System.Diagnostics;
using System.Linq;
using static Mockolate.With;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer getter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class IndexerGetterAccess(int index, object?[] parameters) : IndexerAccess(index, parameters)
{
	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"[{Index}] get indexer [{string.Join(", ", Parameters.Select(p => p?.ToString() ?? "null"))}]";
}
