using System.Collections.Generic;
using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Keeps track of the interactions on the <see cref="Mock{T}" /> and its verifications.
/// </summary>
public class MockChecks
{
	private readonly List<IInteraction> _interactions = [];

	/// <summary>
	///     The number of interactions contained in the collection.
	/// </summary>
	public int Count => _interactions.Count;

	/// <summary>
	///     The registered interactions of the mock.
	/// </summary>
	public IEnumerable<IInteraction> Interactions => _interactions;

	internal IInteraction RegisterInvocation(IInteraction interaction)
	{
		_interactions.Add(interaction);
		return interaction;
	}
}
