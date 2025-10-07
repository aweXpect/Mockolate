using System;
using System.Collections.Generic;
using System.Linq;
using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     The expectation contains the matching interactions for verification.
/// </summary>
public class CheckResult<TMock>
{
	private readonly Checks _checks;
	private readonly IInteraction[] _interactions;
	private readonly TMock _mock;

	/// <summary>
	///     The expectation of this check result.
	/// </summary>
	public string Expectation { get; }

	/// <inheritdoc cref="CheckResult{TMock}" />
	public CheckResult(TMock mock, Checks checks, IInteraction[] interactions, string expectation)
	{
		_mock = mock;
		_checks = checks;
		_interactions = interactions;
		Expectation = expectation;
	}

	/// <summary>
	///     Supports fluent chaining of verifications in a given order.
	/// </summary>
	public bool Then(params Func<TMock, CheckResult<TMock>>[] orderedChecks)
	{
		CheckResult<TMock> result = this;
		List<IInteraction> verified = [];
		foreach (Func<TMock, CheckResult<TMock>>? check in orderedChecks)
		{
			if (result._interactions.Length == 0)
			{
				_checks.Verified(verified);
				return false;
			}

			verified.AddRange(result._interactions);
			_checks.After(result._interactions.Min(x => x.Index));
			result = check(_mock);
		}

		_checks.Verified(verified);
		return result._interactions.Length >= 1;
	}

	/// <summary>
	/// Verifies that the specified <paramref name="predicate"/> holds true for the current set of interactions.
	/// </summary>
	public bool Verify(Func<IInteraction[], bool> predicate)
	{
		_checks.Verified(_interactions);
		return predicate(_interactions);
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
