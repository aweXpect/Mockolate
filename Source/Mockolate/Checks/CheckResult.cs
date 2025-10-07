using System;
using System.Collections.Generic;
using System.Linq;
using Mockolate.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     The expectation contains the matching interactions for verification.
/// </summary>
public class CheckResult<TMock>
{
	private readonly MockInteractions _interactions;
	private readonly IInteraction[] _matchingInteractions;
	private readonly TMock _mock;

	/// <summary>
	///     The expectation of this check result.
	/// </summary>
	public string Expectation { get; }

	/// <inheritdoc cref="CheckResult{TMock}" />
	public CheckResult(TMock mock, MockInteractions interactions, IInteraction[] matchingInteractions, string expectation)
	{
		_mock = mock;
		_interactions = interactions;
		_matchingInteractions = matchingInteractions;
		Expectation = expectation;
	}

	/// <summary>
	///     Supports fluent chaining of verifications in a given order.
	/// </summary>
	public bool Then(params Func<TMock, CheckResult<TMock>>[] orderedChecks)
	{
		CheckResult<TMock> result = this;
		List<IInteraction> verified = [];
		int after = -1;
		foreach (Func<TMock, CheckResult<TMock>>? check in orderedChecks)
		{
			if (!result._matchingInteractions.Any(x => x.Index > after))
			{
				_interactions.Verified(verified);
				return false;
			}

			verified.AddRange(result._matchingInteractions.Where(x => x.Index > after));
			after = result._matchingInteractions.Min(x => x.Index);
			result = check(_mock);
		}

		_interactions.Verified(verified);
		return result._matchingInteractions.Any(x => x.Index > after);
	}

	/// <summary>
	/// Verifies that the specified <paramref name="predicate"/> holds true for the current set of interactions.
	/// </summary>
	public bool Verify(Func<IInteraction[], bool> predicate)
	{
		_interactions.Verified(_matchingInteractions);
		return predicate(_matchingInteractions);
	}
}

/// <summary>
///     The expectation contains the matching interactions for verification.
/// </summary>
public static class CheckResult
{
	/// <summary>
	///     A property expectation returns the getter or setter <see cref="CheckResult" /> for the given
	///     <paramref name="propertyName" />.
	/// </summary>
	public class Property<TMock, TProperty>(IMockAccessed<TMock> mockAccessed, string propertyName)
	{
		/// <summary>
		///     The expectation for the property getter access.
		/// </summary>
		public CheckResult<TMock> Getter() => mockAccessed.PropertyGetter(propertyName);

		/// <summary>
		///     The expectation for the property setter access matching the specified <paramref name="value" />.
		/// </summary>
		public CheckResult<TMock> Setter(With.Parameter<TProperty> value)
			=> mockAccessed.PropertySetter(propertyName, value);
	}

#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     An event expectation returns the subscription or unsubscription <see cref="CheckResult" /> for the given
	///     <paramref name="eventName" />.
	/// </summary>
	public class Event<TMock, TEvent>(IMockEvent<TMock> mockEvent, string eventName)
	{
		/// <summary>
		///     The expectation for the subscriptions for the event.
		/// </summary>
		public CheckResult<TMock> Subscribed() => mockEvent.Subscribed(eventName);

		/// <summary>
		///     The expectation for the unsubscriptions from the event.
		/// </summary>
		public CheckResult<TMock> Unsubscribed() => mockEvent.Unsubscribed(eventName);
	}
#pragma warning restore S2326 // Unused type parameters should be removed
}
