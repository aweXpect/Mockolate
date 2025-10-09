using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Interface for hiding some implementation details of <see cref="MethodSetup" />.
/// </summary>
public interface IIndexerSetup
{
	/// <summary>
	///     The number of matching getter invocations on the mock.
	/// </summary>
	int GetterInvocationCount { get; }

	/// <summary>
	///     The number of matching setter invocations on the mock.
	/// </summary>
	int SetterInvocationCount { get; }

	/// <summary>
	///     Checks if the <paramref name="invocation" /> matches the setup.
	/// </summary>
	bool Matches(IInteraction invocation);
}
