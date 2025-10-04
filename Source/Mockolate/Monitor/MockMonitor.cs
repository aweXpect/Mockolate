using System;
using System.Linq;
using Mockolate.Checks;

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
	private readonly MockInvocations _monitoredInvocations;

	/// <summary>
	///     The invocations that were recorded during the monitoring session.
	/// </summary>
	protected MockInvocations Invocations { get; }
	private int _monitoringStart = -1;

	/// <inheritdoc cref="MockMonitor{T}" />
	public MockMonitor(IMock mock)
	{
		_monitoredInvocations = mock.Invocations;
		Invocations = new();
	}

	/// <summary>
	///     Begins monitoring invocations and returns a scope that ends monitoring when disposed.
	/// </summary>
	/// <remarks>
	///     Dispose the returned object to stop monitoring and finalize the session.
	/// </remarks>
	/// <returns>
	///     An <see cref="IDisposable"/> that ends the monitoring session when disposed.
	/// </returns>
	public IDisposable Run()
	{
		if (_monitoringStart >= 0)
		{
			throw new InvalidOperationException("Monitoring is already running. Dispose the previous scope before starting a new one.");
		}

		_monitoringStart = _monitoredInvocations.Count;
		return new MonitorScope(() => this.Stop());
	}

	internal void Stop()
	{
		if (_monitoringStart >= 0)
		{
			foreach (var invocation in _monitoredInvocations.Invocations.Skip(_monitoringStart))
			{
				Invocations.RegisterInvocation(invocation);
			}
		}
		_monitoringStart = -1;
	}

	private sealed class MonitorScope(Action callback) : IDisposable
	{
		public void Dispose()
		{
			callback();
		}
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
public class MockMonitor<T> : MockMonitor
{
	/// <summary>
	///     Check which properties were accessed on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockAccessed<T> Accessed { get; }

	/// <summary>
	///     Check which events were subscribed or unsubscribed on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockEvent<T> Event { get; }

	/// <summary>
	///     Check which methods got invoked on the mocked instance for <typeparamref name="T"/>.
	/// </summary>
	public MockInvoked<T> Invoked { get; }

	/// <inheritdoc cref="MockMonitor{T}" />
	public MockMonitor(IMock mock) : base(mock)
	{
		Accessed = new MockAccessed<T>(Invocations);
		Event = new MockEvent<T>(Invocations);
		Invoked = new MockInvoked<T>(Invocations);
	}
}
