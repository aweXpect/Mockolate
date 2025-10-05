using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Allows registration of <see cref="IInteraction" /> in the mock.
/// </summary>
public interface IMockAccessed
{
	/// <summary>
	///     Counts the invocations for the getter of property with the given <paramref name="propertyName" />.
	/// </summary>
	CheckResult PropertyGetter(string propertyName);

	/// <summary>
	///     Counts the invocations for the setter of property with the given <paramref name="propertyName" /> with the matching
	///     <paramref name="value" />.
	/// </summary>
	CheckResult PropertySetter(string propertyName, With.Parameter value);
}
