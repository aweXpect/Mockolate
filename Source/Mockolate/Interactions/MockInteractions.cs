using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mockolate.Interactions;

/// <summary>
///     Keeps track of the interactions on the mock and its verifications.
/// </summary>
[DebuggerDisplay("{_interactions.Count} interactions")]
[DebuggerNonUserCode]
public class MockInteractions : IEnumerable<IInteraction>, IMockInteractions
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IInteraction> _interactions = [];
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly object _lock = new();
	private int _index = -1;
	private List<IInteraction>? _missingVerification;

	/// <summary>
	///     The number of interactions contained in the collection.
	/// </summary>
	public int Count
	{
		get
		{
			lock (_lock)
			{
				return _interactions.Count;
			}
		}
	}

	/// <inheritdoc cref="IEnumerable{IInteraction}.GetEnumerator()" />
	public IEnumerator<IInteraction> GetEnumerator()
		=> _interactions.GetEnumerator();

	/// <inheritdoc cref="IEnumerable.GetEnumerator()" />
	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();

	/// <inheritdoc cref="IMockInteractions.RegisterInteraction{TInteraction}(TInteraction)" />
	TInteraction IMockInteractions.RegisterInteraction<TInteraction>(TInteraction interaction)
	{
		if (interaction is not ISettableInteraction settableInteraction)
		{
			throw new ArgumentException("Only settable interactions can be registered.", nameof(interaction));
		}

		lock (_lock)
		{
			_missingVerification?.Add(interaction);
			settableInteraction.SetIndex(++_index);
			_interactions.Add(interaction);
		}

		InteractionAdded?.Invoke(this, EventArgs.Empty);
		return interaction;
	}

	/// <summary>
	///     Gets the unverified interactions of the mock.
	/// </summary>
	public IReadOnlyCollection<IInteraction> GetUnverifiedInteractions()
	{
		lock (_lock)
		{
			_missingVerification ??= _interactions.ToList();
			return _missingVerification;
		}
	}

	internal event EventHandler? InteractionAdded;
	internal event EventHandler? OnClearing;

	internal void Verified(IEnumerable<IInteraction> interactions)
	{
		lock (_lock)
		{
			_missingVerification ??= _interactions.ToList();
			foreach (IInteraction interaction in interactions)
			{
				_missingVerification.Remove(interaction);
			}
		}
	}

	internal void Clear()
	{
		lock (_lock)
		{
			_missingVerification = null;
			_interactions.Clear();
			_index = -1;
		}

		OnClearing?.Invoke(this, EventArgs.Empty);
	}
}
