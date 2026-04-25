using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
#if NET10_0_OR_GREATER
using System.Threading;
#endif

namespace Mockolate.Interactions;

/// <summary>
///     Keeps track of the interactions on the mock and its verifications.
/// </summary>
[DebuggerDisplay("{_interactions.Count} interactions")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class MockInteractions : IReadOnlyCollection<IInteraction>, IMockInteractions
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IInteraction> _interactions = [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#if NET10_0_OR_GREATER
	private readonly Lock _listLock = new();
	private readonly Lock _verifiedLock = new();
#else
	private readonly object _listLock = new();

	private readonly object _verifiedLock = new();
#endif

	private HashSet<IInteraction>? _verified;

	/// <inheritdoc cref="IMockInteractions.SkipInteractionRecording" />
	public bool SkipInteractionRecording { get; init; } = false;

	/// <inheritdoc cref="IMockInteractions.RegisterInteraction{TInteraction}(TInteraction)" />
	TInteraction IMockInteractions.RegisterInteraction<TInteraction>(TInteraction interaction)
	{
		if (SkipInteractionRecording)
		{
			return interaction;
		}

		lock (_listLock)
		{
			_interactions.Add(interaction);
		}

		InteractionAdded?.Invoke(this, EventArgs.Empty);
		return interaction;
	}

	/// <summary>
	///     The number of interactions contained in the collection.
	/// </summary>
	public int Count
	{
		get
		{
			lock (_listLock)
			{
				return _interactions.Count;
			}
		}
	}

	/// <inheritdoc cref="IEnumerable{IInteraction}.GetEnumerator()" />
	public IEnumerator<IInteraction> GetEnumerator()
		=> ((IEnumerable<IInteraction>)Snapshot()).GetEnumerator();

	/// <inheritdoc cref="IEnumerable.GetEnumerator()" />
	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();

	/// <summary>
	///     Gets the unverified interactions of the mock.
	/// </summary>
	public IReadOnlyCollection<IInteraction> GetUnverifiedInteractions()
	{
		IInteraction[] snapshot = Snapshot();
		lock (_verifiedLock)
		{
			if (_verified is null || _verified.Count == 0)
			{
				return snapshot;
			}

			List<IInteraction> result = new(snapshot.Length);
			foreach (IInteraction interaction in snapshot)
			{
				if (!_verified.Contains(interaction))
				{
					result.Add(interaction);
				}
			}

			return result;
		}
	}

	/// <inheritdoc cref="IMockInteractions.InteractionAdded" />
	public event EventHandler? InteractionAdded;

	/// <inheritdoc cref="IMockInteractions.OnClearing" />
	public event EventHandler? OnClearing;

	void IMockInteractions.Verified(IEnumerable<IInteraction> interactions)
	{
		lock (_verifiedLock)
		{
			// Stryker disable once Collection : seeding _verified with { default } adds a null entry that HashSet.Contains never matches against a real interaction, leaving GetUnverifiedInteractions behavior identical.
			_verified ??= [];
			foreach (IInteraction interaction in interactions)
			{
				_verified.Add(interaction);
			}
		}
	}

	/// <inheritdoc cref="IMockInteractions.Clear" />
	public void Clear()
	{
		lock (_listLock)
		{
			_interactions.Clear();
		}

		// Stryker disable once Block : keeping the stale _verified set after Clear never causes an unverified interaction to appear verified because Clear also empties _interactions and any later-added interaction is a fresh reference not present in the stale set.
		lock (_verifiedLock)
		{
			_verified = null;
		}

		OnClearing?.Invoke(this, EventArgs.Empty);
	}

	private IInteraction[] Snapshot()
	{
		lock (_listLock)
		{
			return _interactions.ToArray();
		}
	}
}
