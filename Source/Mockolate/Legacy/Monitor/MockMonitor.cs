using System;
using System.Linq;
using Mockolate.Interactions;
using Mockolate.Legacy.Verify;

namespace Mockolate.Legacy.Monitor;

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
			foreach (IInteraction? interaction in _monitoredInvocations.Interactions.Skip(_monitoringStart))
			{
				((IMockInteractions)Interactions).RegisterInteraction(interaction);
			}
		}

		_monitoringStart = -1;
	}

	/// <summary>
	///     Updates the interactions while the mock monitor is running.
	/// </summary>
	protected void UpdateInteractions()
	{
		if (_monitoringStart >= 0)
		{
			foreach (IInteraction? interaction in _monitoredInvocations.Interactions.Skip(_monitoringStart))
			{
				((IMockInteractions)Interactions).RegisterInteraction(interaction);
				_monitoringStart = interaction.Index + 1;
			}
		}
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
public sealed class MockMonitor<T, TMock> : MockMonitor
	where TMock : IMock
{
	private readonly IMockVerify<T, TMock> _verify;

	/// <inheritdoc cref="MockMonitor{T, TMock}" />
	public MockMonitor(TMock mock) : base(mock)
	{
		_verify = new MockVerify<T, TMock>(Interactions, mock, mock.Prefix);
	}

	/// <summary>
	///     Verifies the interactions with the mocked subject of <typeparamref name="T" />.
	/// </summary>
	public IMockVerify<T, TMock> Verify
	{
		get
		{
			UpdateInteractions();
			return _verify;
		}
	}
}
