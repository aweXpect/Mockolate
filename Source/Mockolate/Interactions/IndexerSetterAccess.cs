using System.Diagnostics;
using System.Linq;
using Mockolate.Parameters;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer setter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerSetterAccess(INamedParameterValue[] parameters, INamedParameterValue value) : IndexerAccess(parameters)
{
	/// <summary>
	///     The value the indexer was being set to.
	/// </summary>
	public INamedParameterValue Value { get; } = value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"set indexer [{string.Join(", ", Parameters.Select(p => p.ToString()))}] to {Value}";
}
