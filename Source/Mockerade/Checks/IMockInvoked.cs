namespace Mockerade.Checks;

/// <summary>
///     Allows registration of <see cref="Invocation" /> in the mock.
/// </summary>
public interface IMockInvoked
{
	/// <summary>
	/// Counts the invocations of a method with the given <paramref name="methodName"/> and matching <paramref name="parameters"/>.
	/// </summary>
	Invocation[] Method(string methodName, params With.Parameter[] parameters);
}
