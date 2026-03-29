using System;
using System.Diagnostics;
using Mockolate.Parameters;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer.
/// </summary>
[DebuggerNonUserCode]
public abstract class IndexerAccess(NamedParameterValue[] parameters) : IInteraction, ISettableInteraction
{
	private int? _index;
	/// <summary>
	///     The parameters of the indexer.
	/// </summary>
	public NamedParameterValue[] Parameters { get; } = parameters;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index => _index.GetValueOrDefault();

	void ISettableInteraction.SetIndex(int value) => _index ??= value;
}
