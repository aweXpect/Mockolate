namespace Mockolate.Checks;

/// <summary>
/// The result of a check containing the matching invocations.
/// </summary>
public interface ICheckResult
{
	/// <summary>
	/// The matching invocations.
	/// </summary>
	Invocation[] Invocations { get; }
}
