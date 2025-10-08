using System;
using System.Linq;
using Mockolate.Checks;
using Mockolate.Interactions;

namespace Mockolate.Monitor;

/// <summary>
///     Provides monitoring capabilities for a mocked instance of the specified type, allowing inspection of accessed
///     properties, invoked methods, and event subscriptions.
/// </summary>
/// <remarks>
///     Use this class to track and analyze interactions with a mock, such as which members were accessed or
///     which events were subscribed to, during a test session. Monitoring is session-based; begin a session with the Run
///     method and dispose the returned scope to finalize monitoring.
/// </remarks>
public abstract class MockMonitor
{
	private readonly MockInteractions _monitoredInvocations;
	private int _monitoringStart = -1;

	/// <inheritdoc cref="MockMonitor" />
	protected MockMonitor(IMock mock)
	{
		_monitoredInvocations = mock.Interactions;
		Interactions = new MockInteractions();
	}

	/// <summary>
	///     The interactions that were recorded during the monitoring session.
	/// </summary>
	protected MockInteractions Interactions { get; }

	/// <summary>
	///     Begins monitoring interactions and returns a scope that ends monitoring when disposed.
	/// </summary>
	/// <remarks>
	///     Dispose the returned object to stop monitoring and finalize the session.
	/// </remarks>
	/// <returns>
	///     An <see cref="IDisposable" /> that ends the monitoring session when disposed.
	/// </returns>
	public IDisposable Run()
	{
		if (_monitoringStart >= 0)
		{
			throw new InvalidOperationException(
				"Monitoring is already running. Dispose the previous scope before starting a new one.");
		}

		_monitoringStart = _monitoredInvocations.Count;
		return new MonitorScope(() => Stop());
	}

	internal void Stop()
	{
		if (_monitoringStart >= 0)
		{
			foreach (IInteraction? invocation in _monitoredInvocations.Interactions.Skip(_monitoringStart))
			{
				((IMockInteractions)Interactions).RegisterInteraction(invocation);
			}
		}

		_monitoringStart = -1;
	}

	private sealed class MonitorScope(Action callback) : IDisposable
	{
		public void Dispose() => callback();
	}
}

/// <summary>
///     Provides monitoring capabilities for a mocked instance of the specified type, allowing inspection of accessed
///     properties, invoked methods, and event subscriptions.
/// </summary>
/// <remarks>
///     Use this class to track and analyze interactions with a mock, such as which members were accessed or
///     which events were subscribed to, during a test session. Monitoring is session-based; begin a session with the Run
///     method and dispose the returned scope to finalize monitoring.
/// </remarks>
public class MockMonitor<T, TMock> : MockMonitor
	where TMock : IMock
{
	/// <inheritdoc cref="MockMonitor{T, TMock}" />
	public MockMonitor(TMock mock) : base(mock)
	{
		Accessed = new MockAccessed<T, TMock>(Interactions, mock);
		Event = new MockEvent<T, TMock>(Interactions, mock);
		Invoked = new MockInvoked<T, TMock>(Interactions, mock);
	}

	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T" />.
	/// </summary>
	public MockAccessed<T, TMock> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T" />.
	/// </summary>
	public MockEvent<T, TMock> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T" />.
	/// </summary>
	public MockInvoked<T, TMock> Invoked { get; }
}
