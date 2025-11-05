namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Get results for property get access on the mock.
/// </summary>
public interface IMockGot<TMock>
{
	/// <summary>
	///     Counts the getter accesses of property <paramref name="propertyName" />.
	/// </summary>
	VerificationResult<TMock> Property(string propertyName);
}
#pragma warning restore S2326 // Unused type parameters should be removed
