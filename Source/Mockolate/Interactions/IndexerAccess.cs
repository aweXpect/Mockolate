using System.Diagnostics;
using Mockolate.Parameters;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public abstract class IndexerAccess(INamedParameterValue[] parameters) : IInteraction, ISettableInteraction
{
	private int? _index;
	/// <summary>
	///     The parameters of the indexer.
	/// </summary>
	public INamedParameterValue[] Parameters { get; } = parameters;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index => _index.GetValueOrDefault();

	void ISettableInteraction.SetIndex(int value) => _index ??= value;
}
