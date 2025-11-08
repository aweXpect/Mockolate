namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Get results for property set access on the mock.
/// </summary>
public interface IMockSet<TMock>
{
	/// <summary>
	///     Counts the setter accesses of property <paramref name="propertyName" />
	///     with the matching <paramref name="value" />.
	/// </summary>
	VerificationResult<TMock> Property(string propertyName, Match.IParameter value);
}
#pragma warning restore S2326 // Unused type parameters should be removed
