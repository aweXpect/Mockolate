using Mockolate.Exceptions;
using Mockolate.Setup;
using Mockolate.Verify;

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

	/// <summary>
	///     Verifies the method invocations for the <paramref name="setup" /> on the mock.
	/// </summary>
	public static VerificationResult<T> InvokedSetup<T>(this IMockVerify<T> verify, IMethodSetup setup)
	{
		if (setup is IVerifiableMethodSetup verifiableMethodSetup &&
		    verify is Mock<T> mock)
		{
			return mock.Registrations.Method(mock.Subject, verifiableMethodSetup.GetMatch());
		}

		throw new MockException("The subject is no mock.");
	}
}
