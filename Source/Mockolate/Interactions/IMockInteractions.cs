using System;
using System.Collections.Generic;

namespace Mockolate.Interactions;

/// <summary>
///     Keeps track of the interactions on the mock and its verifications.
/// </summary>
public interface IMockInteractions : IReadOnlyCollection<IInteraction>
{
	/// <summary>
	///     Whether interaction recording is suppressed. When <see langword="true" />,
	///     <see cref="RegisterInteraction{TInteraction}(TInteraction)" /> is a no-op and
	///     attempts to verify throw a <see cref="Mockolate.Exceptions.MockException" />.
	/// </summary>
	/// <remarks>
	///     Mirrors <see cref="MockBehavior.SkipInteractionRecording" /> at construction time.
	/// </remarks>
	bool SkipInteractionRecording { get; }

	/// <summary>
	///     Registers an <paramref name="interaction" />.
	/// </summary>
	TInteraction RegisterInteraction<TInteraction>(TInteraction interaction)
		where TInteraction : IInteraction;

	/// <summary>
	///     Raised after a new interaction has been appended to the collection.
	/// </summary>
	event EventHandler? InteractionAdded;

	/// <summary>
	///     Raised after all recorded interactions have been cleared and the verified-set bookkeeping has been reset.
	/// </summary>
	event EventHandler? OnClearing;

	/// <summary>
	///     Clears all recorded interactions and resets the verified-set bookkeeping.
	/// </summary>
	void Clear();

	/// <summary>
	///     Gets the interactions that have not yet been matched by a successful verification.
	/// </summary>
	IReadOnlyCollection<IInteraction> GetUnverifiedInteractions();

	/// <summary>
	///     Marks the given <paramref name="interactions" /> as verified so they are excluded from
	///     future calls to <see cref="GetUnverifiedInteractions" />.
	/// </summary>
	internal void Verified(IEnumerable<IInteraction> interactions);
}
