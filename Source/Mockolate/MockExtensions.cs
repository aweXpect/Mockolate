using Mockolate.Exceptions;
using Mockolate.Setup;
using Mockolate.Verify;

namespace Mockolate;

/// <summary>
///     Extension methods for mock objects.
/// </summary>
public static class MockExtensions
{
	/// <inheritdoc cref="MockExtensions" />
	extension<T>(IMockSetup<T> mock) where T : class
	{
		/// <summary>
		///     Clears all interactions recorded by the mock object.
		/// </summary>
		public void ClearAllInteractions()
		{
			if (mock is IHasMockRegistration hasMockRegistration)
			{
				hasMockRegistration.Registrations.ClearAllInteractions();
			}
		}
	}

	/// <inheritdoc cref="MockExtensions" />
	extension<T>(IMockVerify<T> verify)
	{
		/// <summary>
		///     Verifies the method invocations for the <paramref name="setup" /> on the mock.
		/// </summary>
		public VerificationResult<T> InvokedSetup(IMethodSetup setup)
		{
			if (verify is not Mock<T> mock)
			{
				throw new MockException("The subject is no mock subject.");
			}

			if (setup is not IVerifiableMethodSetup verifiableMethodSetup)
			{
				throw new MockException("The setup is not verifiable.");
			}

			return mock.Registrations.Method(mock.Subject, verifiableMethodSetup.GetMatch());
		}
	}
}
