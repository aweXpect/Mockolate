using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#if NET10_0_OR_GREATER
using System.Threading;
#endif

namespace Mockolate.Interactions;

/// <summary>
///     Keeps track of the interactions on the mock and its verifications.
/// </summary>
[DebuggerDisplay("{_interactions.Count} interactions")]
[DebuggerNonUserCode]
public class MockInteractions : IReadOnlyCollection<IInteraction>, IMockInteractions
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly List<IInteraction> _interactions = [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#if NET10_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

	private int _index = -1;
	private List<IInteraction>? _missingVerification;

	/// <inheritdoc cref="IMockInteractions.RegisterInteraction{TInteraction}(TInteraction)" />
	TInteraction IMockInteractions.RegisterInteraction<TInteraction>(TInteraction interaction)
	{
		if (interaction is not ISettableInteraction settableInteraction)
		{
			// ReSharper disable once LocalizableElement
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
	{
		lock (_lock)
		{
			return _interactions.ToList().GetEnumerator();
		}
	}

	/// <inheritdoc cref="IEnumerable.GetEnumerator()" />
	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();

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
