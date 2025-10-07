using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Mockolate.Interactions;

/// <summary>
///     Keeps track of the interactions on the <see cref="Mock{T}" /> and its verifications.
/// </summary>
[DebuggerDisplay("{_interactions}")]
public class MockInteractions
{
	private readonly ConcurrentDictionary<int, IInteraction> _interactions = [];
	private int _index = -1;
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
	public IEnumerable<IInteraction> Interactions => _interactions.Values.OrderBy(x => x.Index);

	/// <summary>
	///     Gets the next index for an interaction.
	/// </summary>
	public int GetNextIndex() => Interlocked.Increment(ref _index);

	internal IInteraction RegisterInteraction(IInteraction interaction)
	{
		_interactions.TryAdd(interaction.Index, interaction);
		return interaction;
	}

	internal void Verified(IEnumerable<IInteraction> interactions)
	{
		_missingVerification ??= _interactions.Values.OrderBy(x => x.Index).ToList();
		foreach (IInteraction? interaction in interactions)
		{
			_missingVerification.Remove(interaction);
		}
	}
}
