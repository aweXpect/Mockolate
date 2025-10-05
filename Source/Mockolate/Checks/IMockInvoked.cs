using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Get results for method invocations on the mock.
/// </summary>
public interface IMockInvoked
{
	/// <summary>
	///     Counts the invocations of method <paramref name="methodName" /> with matching <paramref name="parameters" />.
	/// </summary>
	CheckResult Method(string methodName, params With.Parameter[] parameters);
}
