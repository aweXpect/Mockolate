using System.Linq;
using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Check which events were subscribed or unsubscribed on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class MockEvent<T, TMock>(Checks checks, TMock mock) : IMockEvent<TMock>
{
	private readonly TMock _mock = mock;
	private readonly Checks _checks = checks;

	/// <inheritdoc cref="IMockEvent{T}.Subscribed(string)" />
	CheckResult<TMock> IMockEvent<TMock>.Subscribed(string eventName) => new(_mock, _checks,
		_checks.Interactions
			.OfType<EventSubscription>()
			.Where(@event => @event.Name.Equals(eventName))
			.ToArray());

	/// <inheritdoc cref="IMockEvent{T}.Unsubscribed(string)" />
	CheckResult<TMock> IMockEvent<TMock>.Unsubscribed(string eventName) => new(_mock, _checks,
		_checks.Interactions
			.OfType<EventUnsubscription>()
			.Where(@event => @event.Name.Equals(eventName))
			.ToArray());

	/// <summary>
	///     A proxy implementation of <see cref="IMockEvent{T}" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockEvent<TMock> inner, Checks checks, TMock mock)
		: MockEvent<T, TMock>(checks, mock), IMockEvent<TMock>
	{
		/// <inheritdoc cref="IMockEvent{T}.Subscribed(string)" />
		CheckResult<TMock> IMockEvent<TMock>.Subscribed(string eventName)
			=> inner.Subscribed(eventName);

		/// <inheritdoc cref="IMockEvent{T}.Unsubscribed(string)" />
		CheckResult<TMock> IMockEvent<TMock>.Unsubscribed(string eventName)
			=> inner.Unsubscribed(eventName);
	}

	/// <summary>
	///     Check which protected events were subscribed or unsubscribed on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockEvent<TMock> inner, Checks checks, TMock mock)
		: MockEvent<T, TMock>(checks, mock), IMockEvent<TMock>
	{
		/// <inheritdoc cref="IMockEvent{T}.Subscribed(string)" />
		CheckResult<TMock> IMockEvent<TMock>.Subscribed(string eventName)
			=> inner.Subscribed(eventName);

		/// <inheritdoc cref="IMockEvent{T}.Unsubscribed(string)" />
		CheckResult<TMock> IMockEvent<TMock>.Unsubscribed(string eventName)
			=> inner.Unsubscribed(eventName);
	}
}
