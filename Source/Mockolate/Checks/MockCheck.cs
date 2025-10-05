using System.Linq;
using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Additional checks on the mocked instance for <typeparamref name="T" />.
/// </summary>
public class MockCheck<T>(Checks checks)
{
	/// <summary>
	///     Gets a value indicating whether all expected interactions have been verified.
	/// </summary>
	public bool AllInteractionsVerified => !checks.HasMissingVerifications;
}
