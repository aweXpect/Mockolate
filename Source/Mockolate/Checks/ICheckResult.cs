using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     The result of a check containing the matching invocations.
/// </summary>
public interface ICheckResult
{
	/// <summary>
	///     The matching invocations.
	/// </summary>
	IInteraction[] Interactions { get; }
}
