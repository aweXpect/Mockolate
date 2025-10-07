namespace Mockolate.Interactions;

/// <summary>
///     Marker interface for interactions with the <see cref="Mock{T}" />.
/// </summary>
public interface IInteraction
{
	/// <summary>
	///     The index of the interaction.
	/// </summary>
	int Index { get; }
}
