using Mockolate.Setup;

namespace Mockolate;

/// <summary>
///     Extension methods for mock objects.
/// </summary>
public static class MockExtensions
{
	/// <summary>
	///     Clears all interactions recorded by the mock object.
	/// </summary>
	public static void ClearAllInteractions<T>(this IMockSetup<T> mock)
		where T : class
	{
		if (mock is IHasMockRegistration hasMockRegistration)
		{
			hasMockRegistration.Registrations.ClearAllInteractions();
		}
	}
}
