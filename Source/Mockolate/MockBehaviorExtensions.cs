namespace Mockolate;

/// <summary>
///     Extension methods for <see cref="MockBehavior" />.
/// </summary>
public static class MockBehaviorExtensions
{
	/// <inheritdoc cref="MockBehaviorExtensions" />
	extension(MockBehavior mockBehavior)
	{
		/// <summary>
		///     Specifies if the base class implementation should be called, and
		///     its return values used as default values.
		/// </summary>
		/// <remarks>
		///     Sets the <see cref="MockBehavior.CallBaseClass" /> to <paramref name="callBaseClass" />.
		/// </remarks>
		public MockBehavior CallingBaseClass(bool callBaseClass = true)
			=> mockBehavior with
			{
				CallBaseClass = callBaseClass,
			};

		/// <summary>
		///     Specifies whether an exception is thrown when an operation is attempted without prior setup.
		/// </summary>
		/// <remarks>
		///     Sets the <see cref="MockBehavior.ThrowWhenNotSetup" /> to <paramref name="throwWhenNotSetup" />.
		/// </remarks>
		public MockBehavior ThrowingWhenNotSetup(bool throwWhenNotSetup = true)
			=> mockBehavior with
			{
				ThrowWhenNotSetup = throwWhenNotSetup,
			};
	}
}
