using System.Diagnostics.Contracts;

namespace Mockerade.Checks;

/// <summary>
///     The expectation contains the matching invocations for verification.
/// </summary>
public class CheckResult : ICheckResult
{
	private readonly Invocation[] _invocations;

	/// <inheritdoc cref="CheckResult" />
	public CheckResult(Invocation[] invocations)
	{
		this._invocations = invocations;
	}

	/// <inheritdoc cref="ICheckResult.Invocations" />
	Invocation[] ICheckResult.Invocations => _invocations;

	/// <summary>
	///     …at least the expected number of <paramref name="times" />.
	/// </summary>
	[Pure]
	public bool AtLeast(int times) => _invocations.Length >= times;

	/// <summary>
	///     …at least once.
	/// </summary>
	[Pure]
	public bool AtLeastOnce() => _invocations.Length >= 1;

	/// <summary>
	///     …at most the expected number of <paramref name="times" />.
	/// </summary>
	[Pure]
	public bool AtMost(int times) => _invocations.Length <= times;

	/// <summary>
	///     …at most once.
	/// </summary>
	[Pure]
	public bool AtMostOnce() => _invocations.Length <= 1;

	/// <summary>
	///     …exactly the expected number of <paramref name="times" />.
	/// </summary>
	[Pure]
	public bool Exactly(int times) => _invocations.Length == times;

	/// <summary>
	///     …never.
	/// </summary>
	[Pure]
	public bool Never() => _invocations.Length == 0;

	/// <summary>
	///     …exactly once.
	/// </summary>
	[Pure]
	public bool Once() => _invocations.Length == 1;

	/// <summary>
	///     A property expectation returns the getter or setter <see cref="CheckResult"/> for the given <paramref name="propertyName"/>.
	/// </summary>
	public class Property<T>(IMockAccessed mockAccessed, string propertyName)
	{
		/// <summary>
		///     The expectation for the property getter invocations.
		/// </summary>
		[Pure]
		public CheckResult Getter() => new CheckResult(mockAccessed.PropertyGetter(propertyName));
		/// <summary>
		///     The expectation for the property setter invocations matching the specified <paramref name="value"/>.
		/// </summary>
		[Pure]
		public CheckResult Setter(With.Parameter<T> value) => new CheckResult(mockAccessed.PropertySetter(propertyName, value));
	}

	/// <summary>
	///     An event expectation returns the subscription or unsubscription <see cref="CheckResult"/> for the given <paramref name="eventName"/>.
	/// </summary>
	public class Event<T>(IMockEvent mockEvent, string eventName)
	{
		/// <summary>
		///     The expectation for the subscription invocations.
		/// </summary>
		[Pure]
		public CheckResult Subscribed() => new CheckResult(mockEvent.Subscribed(eventName));
		/// <summary>
		///     The expectation for the unsubscription invocations.
		/// </summary>
		[Pure]
		public CheckResult Unsubscribed() => new CheckResult(mockEvent.Unsubscribed(eventName));
	}
}
