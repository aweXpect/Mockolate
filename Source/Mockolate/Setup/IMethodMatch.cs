using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     A method match to verify method invocations from a setup.
/// </summary>
public interface IMethodMatch
{
	/// <summary>
	///     Checks if the <paramref name="methodInvocation" /> matches.
	/// </summary>
	bool Matches(MethodInvocation methodInvocation);
}
