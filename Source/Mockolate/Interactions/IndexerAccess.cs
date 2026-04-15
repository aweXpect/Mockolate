using System.Diagnostics;
using Mockolate.Parameters;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public abstract class IndexerAccess(INamedParameterValue[] parameters) : IInteraction
{
	/// <summary>
	///     The parameters of the indexer.
	/// </summary>
	public INamedParameterValue[] Parameters { get; } = parameters;
}
