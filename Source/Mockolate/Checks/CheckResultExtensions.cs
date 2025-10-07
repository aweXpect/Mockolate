namespace Mockolate.Checks;

/// <summary>
///     The expectation contains the matching interactions for verification.
/// </summary>
public static class CheckResultExtensions
{
	/// <summary>
	///     …at least the expected number of <paramref name="times" />.
	/// </summary>
	public static bool AtLeast<TMock>(this CheckResult<TMock> checkResult, int times)
		=> checkResult.Verify(interactions => interactions.Length >= times);

	/// <summary>
	///     …at least once.
	/// </summary>
	public static bool AtLeastOnce<TMock>(this CheckResult<TMock> checkResult)
		=> checkResult.Verify(interactions => interactions.Length >= 1);

	/// <summary>
	///     …at most the expected number of <paramref name="times" />.
	/// </summary>
	public static bool AtMost<TMock>(this CheckResult<TMock> checkResult, int times)
		=> checkResult.Verify(interactions => interactions.Length <= times);

	/// <summary>
	///     …at most once.
	/// </summary>
	public static bool AtMostOnce<TMock>(this CheckResult<TMock> checkResult)
		=> checkResult.Verify(interactions => interactions.Length <= 1);

	/// <summary>
	///     …exactly the expected number of <paramref name="times" />.
	/// </summary>
	public static bool Exactly<TMock>(this CheckResult<TMock> checkResult, int times)
		=> checkResult.Verify(interactions => interactions.Length == times);

	/// <summary>
	///     …never.
	/// </summary>
	public static bool Never<TMock>(this CheckResult<TMock> checkResult)
		=> checkResult.Verify(interactions => interactions.Length == 0);

	/// <summary>
	///     …exactly once.
	/// </summary>
	public static bool Once<TMock>(this CheckResult<TMock> checkResult)
		=> checkResult.Verify(interactions => interactions.Length == 1);
}
