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

	/// <summary>
	///     The expectation of this check result.
	/// </summary>
	public string Expectation { get; }

	/// <summary>
	///     The mock object for which this expectation applies.
	/// </summary>
	public TMock Mock { get; }

	/// <inheritdoc cref="CheckResult{TMock}" />
	public CheckResult(TMock mock, MockInteractions interactions, IInteraction[] matchingInteractions, string expectation)
	{
		Mock = mock;
		_interactions = interactions;
		_matchingInteractions = matchingInteractions;
		Expectation = expectation;
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
