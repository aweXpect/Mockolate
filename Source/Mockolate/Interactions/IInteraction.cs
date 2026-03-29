namespace Mockolate.Interactions;

/// <summary>
///     Marker interface for interactions with the mock.
/// </summary>
public interface IInteraction
{
	/// <summary>
	///     The index of the interaction.
	/// </summary>
	int? Index { get; set; }
}
