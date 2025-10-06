using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Keeps track of the interactions on the <see cref="Mock{T}" /> and its verifications.
/// </summary>
public class Checks
{
	private readonly List<IInteraction> _interactions = [];
	private List<IInteraction>? _missingVerification;
	private int _index = -1;
	private int _after = -1;

	/// <summary>
	///     Gets the next index for an interaction.
	/// </summary>
	public int GetNextIndex() => Interlocked.Increment(ref _index);

	internal void After(int index)
	{
		_after = index;
	}

	/// <summary>
	///     The number of interactions contained in the collection.
	/// </summary>
	public int Count => _interactions.Count - _after - 1;

	/// <summary>
	///     Gets a value indicating whether there are any missing verifications for the current context.
	/// </summary>
	internal bool HasMissingVerifications => _missingVerification is null
		? _interactions.Count > _after + 1
		: _missingVerification.Where(x => x.Index > _after).Any();

	/// <summary>
	///     The registered interactions of the mock.
	/// </summary>
	public IEnumerable<IInteraction> Interactions => _interactions.Where(x => x.Index > _after);

	internal IInteraction RegisterInvocation(IInteraction interaction)
	{
		_interactions.Add(interaction);
		return interaction;
	}

	internal void Verified(IEnumerable<IInteraction> interactions)
	{
		_after = -1;
		_missingVerification ??= _interactions.ToList();
		foreach (var interaction in interactions)
		{
			_missingVerification.Remove(interaction);
		}
	}
}
