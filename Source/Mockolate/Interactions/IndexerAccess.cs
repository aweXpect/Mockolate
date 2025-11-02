namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer.
/// </summary>
public abstract class IndexerAccess(int index, object?[] parameters) : IInteraction
{
	/// <summary>
	///     The parameters of the indexer.
	/// </summary>
	public object?[] Parameters { get; } = parameters;

	/// <inheritdoc cref="IInteraction.Index" />
	public int Index { get; } = index;
}
