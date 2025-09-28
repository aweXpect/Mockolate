using Mockerade.Checks;

namespace Mockerade.Setup;

/// <summary>
/// Interface for hiding some implementation details of <see cref="MethodSetup" />.
/// </summary>
public interface IMethodSetup
{
	/// <summary>
	///     The number of matching invocations on the mock.
	/// </summary>
	int InvocationCount { get; }

	/// <summary>
	///     Checks if the <paramref name="invocation" /> matches the setup.
	/// </summary>
	bool Matches(Invocation invocation);
}
