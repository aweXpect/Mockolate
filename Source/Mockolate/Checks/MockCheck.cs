namespace Mockolate.Checks;

/// <summary>
///     Additional checks on the mocked instance.
/// </summary>
public class MockCheck(Checks checks)
{
	/// <summary>
	///     Gets a value indicating whether all expected interactions have been verified.
	/// </summary>
	public bool AllInteractionsVerified => !checks.HasMissingVerifications;
}
