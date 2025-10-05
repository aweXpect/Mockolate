using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     The result of a check containing the matching interactions.
/// </summary>
public interface ICheckResult
{
	/// <summary>
	///     The matching interactions.
	/// </summary>
	IInteraction[] Interactions { get; }
}
