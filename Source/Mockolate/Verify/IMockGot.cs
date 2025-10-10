namespace Mockolate.Checks;

/// <summary>
///     Get results for property access on the mock.
/// </summary>
public interface IMockGot<TMock>
{
	/// <summary>
	///     Counts the getter accesses of property <paramref name="propertyName" />.
	/// </summary>
	CheckResult<TMock> Property(string propertyName);
}
