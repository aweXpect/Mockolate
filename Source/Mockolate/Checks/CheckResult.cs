using System.Diagnostics.Contracts;
using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     The expectation contains the matching interactions for verification.
/// </summary>
public class CheckResult
{
	private readonly Checks _checks;
	private readonly IInteraction[] _interactions;

	/// <inheritdoc cref="CheckResult" />
	public CheckResult(Checks checks, IInteraction[] interactions)
	{
		_checks = checks;
		_interactions = interactions;
	}

	/// <summary>
	///     …at least the expected number of <paramref name="times" />.
	/// </summary>
	public bool AtLeast(int times)
	{
		_checks.Verified(_interactions);
		return _interactions.Length >= times;
	}

	/// <summary>
	///     …at least once.
	/// </summary>
	public bool AtLeastOnce()
	{
		_checks.Verified(_interactions);
		return _interactions.Length >= 1;
	}

	/// <summary>
	///     …at most the expected number of <paramref name="times" />.
	/// </summary>
	public bool AtMost(int times)
	{
		_checks.Verified(_interactions);
		return _interactions.Length <= times;
	}

	/// <summary>
	///     …at most once.
	/// </summary>
	public bool AtMostOnce()
	{
		_checks.Verified(_interactions);
		return _interactions.Length <= 1;
	}

	/// <summary>
	///     …exactly the expected number of <paramref name="times" />.
	/// </summary>
	public bool Exactly(int times)
	{
		_checks.Verified(_interactions);
		return _interactions.Length == times;
	}

	/// <summary>
	///     …never.
	/// </summary>
	public bool Never()
	{
		_checks.Verified(_interactions);
		return _interactions.Length == 0;
	}

	/// <summary>
	///     …exactly once.
	/// </summary>
	public bool Once()
	{
		_checks.Verified(_interactions);
		return _interactions.Length == 1;
	}

	/// <summary>
	///     A property expectation returns the getter or setter <see cref="CheckResult" /> for the given
	///     <paramref name="propertyName" />.
	/// </summary>
	public class Property<T>(IMockAccessed mockAccessed, string propertyName)
	{
		/// <summary>
		///     The expectation for the property getter access.
		/// </summary>
		public CheckResult Getter() => mockAccessed.PropertyGetter(propertyName);

		/// <summary>
		///     The expectation for the property setter access matching the specified <paramref name="value" />.
		/// </summary>
		public CheckResult Setter(With.Parameter<T> value) => mockAccessed.PropertySetter(propertyName, value);
	}

#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     An event expectation returns the subscription or unsubscription <see cref="CheckResult" /> for the given
	///     <paramref name="eventName" />.
	/// </summary>
	public class Event<T>(IMockEvent mockEvent, string eventName)
	{
		/// <summary>
		///     The expectation for the subscriptions for the event.
		/// </summary>
		public CheckResult Subscribed() => mockEvent.Subscribed(eventName);

		/// <summary>
		///     The expectation for the unsubscriptions from the event.
		/// </summary>
		public CheckResult Unsubscribed() => mockEvent.Unsubscribed(eventName);
	}
#pragma warning restore S2326 // Unused type parameters should be removed
}
