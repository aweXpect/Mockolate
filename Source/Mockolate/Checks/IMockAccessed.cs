using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Get results for property access on the mock.
/// </summary>
public interface IMockAccessed
{
	/// <summary>
	///     Counts the getter accesses of property <paramref name="propertyName" />.
	/// </summary>
	CheckResult PropertyGetter(string propertyName);

	/// <summary>
	///     Counts the setter accesses of property <paramref name="propertyName" /> with the matching <paramref name="value" />.
	/// </summary>
	CheckResult PropertySetter(string propertyName, With.Parameter value);
}
