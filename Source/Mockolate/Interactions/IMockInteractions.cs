namespace Mockolate.Interactions;

/// <summary>
///     Keeps track of the interactions on the <see cref="Mock{T}" /> and its verifications.
/// </summary>
public interface IMockInteractions
{
	/// <summary>
	///     Registers an <paramref name="interaction"/>.
	/// </summary>
	IInteraction RegisterInteraction(IInteraction interaction);
}
