namespace Mockerade.Monitor;

/// <summary>
///     Extension methods for <see cref="MockMonitor{T}"/>.
/// </summary>
public static class MockMonitorExtensions
{
	/// <summary>
	///     Create a mock monitor for the given <paramref name="mock"/> instance.
	/// </summary>
	public static MockMonitor<T> Monitor<T>(this Mock<T> mock)
	{
		return new MockMonitor<T>(mock);
	}
}
