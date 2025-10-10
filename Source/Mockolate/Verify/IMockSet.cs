namespace Mockolate.Checks;

/// <summary>
///     Get results for property access on the mock.
/// </summary>
public interface IMockSet<TMock>
{
	/// <summary>
	///     Counts the setter accesses of property <paramref name="propertyName" />
	///     with the matching <paramref name="value" /> .
	/// </summary>
	CheckResult<TMock> Property(string propertyName, With.Parameter value);
}
