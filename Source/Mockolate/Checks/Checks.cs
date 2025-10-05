using System.Collections.Generic;
using System.Linq;
using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Keeps track of the interactions on the <see cref="Mock{T}" /> and its verifications.
/// </summary>
public class Checks
{
	private readonly List<IInteraction> _interactions = [];
	private List<IInteraction>? _missingVerification;

	/// <summary>
	///     The number of interactions contained in the collection.
	/// </summary>
	public int Count => _interactions.Count;

	/// <summary>
	///     Gets a value indicating whether there are any missing verifications for the current context.
	/// </summary>
	internal bool HasMissingVerifications => _missingVerification is null
		? _interactions.Count > 0
		: _missingVerification.Count > 0;

	/// <summary>
	///     The registered interactions of the mock.
	/// </summary>
	public IEnumerable<IInteraction> Interactions => _interactions;

	internal IInteraction RegisterInvocation(IInteraction interaction)
	{
		_interactions.Add(interaction);
		return interaction;
	}

	internal void Verified(IEnumerable<IInteraction> interactions)
	{
		_missingVerification ??= _interactions.ToList();
		foreach (var interaction in interactions)
		{
			_missingVerification.Remove(interaction);
		}
	}
}
