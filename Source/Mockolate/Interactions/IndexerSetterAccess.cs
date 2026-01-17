using System.Diagnostics;
using System.Linq;
using Mockolate.Parameters;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer setter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class IndexerSetterAccess(int index, NamedParameterValue[] parameters, object? value) : IndexerAccess(index, parameters)
{
	/// <summary>
	///     The value the indexer was being set to.
	/// </summary>
	public object? Value { get; } = value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"[{Index}] set indexer [{string.Join(", ", Parameters.Select(p => p.Value?.ToString() ?? "null"))}] to {Value ?? "null"}";
}
