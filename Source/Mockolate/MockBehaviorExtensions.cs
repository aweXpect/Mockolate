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
	///     Sets the <see cref="MockBehavior.BaseClassBehavior" /> to <see cref="BaseClassBehavior.CallBaseClass" />.
	/// </remarks>
	public static MockBehavior CallingBaseClass(this MockBehavior mockBehavior)
		=> mockBehavior with
		{
			BaseClassBehavior = BaseClassBehavior.CallBaseClass,
		};

	/// <summary>
	///     Does not call the base class implementation.
	/// </summary>
	/// <remarks>
	///     Sets the <see cref="MockBehavior.BaseClassBehavior" /> to <see cref="BaseClassBehavior.IgnoreBaseClass" />.
	/// </remarks>
	public static MockBehavior IgnoringBaseClass(this MockBehavior mockBehavior)
		=> mockBehavior with
		{
			BaseClassBehavior = BaseClassBehavior.IgnoreBaseClass,
		};
}
