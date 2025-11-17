namespace Mockolate;

/// <summary>
///     Extension methods for <see cref="MockBehavior" />.
/// </summary>
public static class MockBehaviorExtensions
{
	/// <summary>
	///     Specifies if the base class implementation should be called, and
	///     its return values used as default values.
	/// </summary>
	/// <remarks>
	///     Sets the <see cref="MockBehavior.CallBaseClass" /> to <paramref name="callBaseClass"/>.
	/// </remarks>
	public static MockBehavior CallingBaseClass(this MockBehavior mockBehavior, bool callBaseClass = true)
		=> mockBehavior with
		{
			CallBaseClass = callBaseClass,
		};
}
