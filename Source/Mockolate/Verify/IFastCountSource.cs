namespace Mockolate.Verify;

/// <summary>
///     Allocation-free count source for the count-only verify fast path. Implementations call into a
///     typed per-member buffer's <c>CountMatching</c> overload so verification terminators that only
///     need a count (<c>Once</c>, <c>Exactly</c>, <c>AtLeast</c>, ...) can avoid materializing
///     <see cref="Mockolate.Interactions.IInteraction" /> instances.
/// </summary>
internal interface IFastCountSource
{
	/// <summary>
	///     Counts recorded interactions whose parameters satisfy the source's captured matchers.
	/// </summary>
	int Count();

	/// <summary>
	///     Counts every recorded interaction, ignoring captured matchers.
	///     Used when <c>AnyParameters()</c> widens the verification to any argument list.
	/// </summary>
	int CountAll();
}
