using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Mockolate.Exceptions;

namespace Mockolate.Interactions;

/// <summary>
///     Keeps track of the interactions on the <see cref="Mock{T}" /> and its verifications.
/// </summary>
[DebuggerDisplay("{_interactions}")]
public class MockInteractions : IMockInteractions
{
	private readonly ConcurrentDictionary<int, IInteraction> _interactions = [];
	private int _index = -1;
	private List<IInteraction>? _missingVerification;

	/// <summary>
	///     The number of interactions contained in the collection.
	/// </summary>
	public int Count
		=> _interactions.Count;

	/// <summary>
	///     The registered interactions of the mock.
	/// </summary>
	public IEnumerable<IInteraction> Interactions
		=> _interactions.Values.OrderBy(x => x.Index);

	/// <inheritdoc cref="IMockInteractions.RegisterInteraction{TInteraction}(TInteraction)" />
	TInteraction IMockInteractions.RegisterInteraction<TInteraction>(TInteraction interaction)
	{
		_missingVerification?.Add(interaction);
		_interactions.TryAdd(interaction.Index, interaction);
		return interaction;
	}

	/// <summary>
	///     Gets the unverified interactions of the mock.
	/// </summary>
	public IReadOnlyCollection<IInteraction> GetUnverifiedInteractions()
	{
		_missingVerification ??= _interactions.Values.OrderBy(x => x.Index).ToList();
		return _missingVerification;
	}

	internal event EventHandler? OnClearing;

	internal int GetNextIndex()
		=> Interlocked.Increment(ref _index);

	internal void Verified(IEnumerable<IInteraction> interactions)
	{
		_missingVerification ??= _interactions.Values.OrderBy(x => x.Index).ToList();
		foreach (IInteraction interaction in interactions)
		{
			_missingVerification.Remove(interaction);
		}
	}

	internal void Clear()
	{
		OnClearing?.Invoke(this, EventArgs.Empty);
		_interactions.Clear();
		_index = -1;
	}
}
