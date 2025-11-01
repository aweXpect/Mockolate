using System;

namespace Mockolate.Monitor;

/// <summary>
///     Extension methods for <see cref="MockMonitor{T, TMock}" />.
/// </summary>
public static class MockMonitorExtensions
{
	/// <summary>
	///     Create a <paramref name="monitor" /> to verify the interaction on the <paramref name="mock" /> during a specific
	///     time frame.
	/// </summary>
	/// <remarks>
	///     Returns an <see cref="IDisposable" /> that can be used to stop the monitoring.
	/// </remarks>
	public static IDisposable Monitor<T>(this Mock<T> mock, out MockMonitor<T, Mock<T>> monitor)
	{
		monitor = new MockMonitor<T, Mock<T>>(mock);
		return monitor.Run();
	}
}
