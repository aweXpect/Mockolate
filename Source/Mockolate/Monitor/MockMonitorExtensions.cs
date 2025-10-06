namespace Mockolate.Monitor;

/// <summary>
///     Extension methods for <see cref="MockMonitor{T, TMock}" />.
/// </summary>
public static class MockMonitorExtensions
{
	/// <summary>
	///     Create a mock monitor for the given <paramref name="mock" /> instance.
	/// </summary>
	public static MockMonitor<T, Mock<T>> Monitor<T>(this Mock<T> mock) => new(mock);
}
