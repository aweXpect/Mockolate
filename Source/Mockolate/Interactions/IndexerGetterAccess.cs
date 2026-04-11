using System.Diagnostics;
using System.Linq;
using Mockolate.Parameters;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer getter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerGetterAccess(INamedParameterValue[] parameters) : IndexerAccess(parameters)
{
	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"[{Index}] get indexer [{string.Join(", ", Parameters.Select(p => p.ToString()))}]";
}
