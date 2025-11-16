namespace Mockolate;

/// <summary>
///     Extension methods for <see cref="MockBehavior" />.
/// </summary>
public static class MockBehaviorExtensions
{
	/// <summary>
	///     Calls the base class implementation, and uses its return values as default values.
	/// </summary>
	/// <remarks>
	///     Sets the <see cref="MockBehavior.CallBaseClass" /> to <see langword="true" />.
	/// </remarks>
	public static MockBehavior CallingBaseClass(this MockBehavior mockBehavior)
		=> mockBehavior with
		{
			CallBaseClass = true,
		};

	/// <summary>
	///     Does not call the base class implementation.
	/// </summary>
	/// <remarks>
	///     Sets the <see cref="MockBehavior.CallBaseClass" /> to <see langword="false" />.
	/// </remarks>
	public static MockBehavior IgnoringBaseClass(this MockBehavior mockBehavior)
		=> mockBehavior with
		{
			CallBaseClass = false,
		};
}
