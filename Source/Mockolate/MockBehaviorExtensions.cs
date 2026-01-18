using System;

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
		///     Specifies if calling the base class implementation should be skipped.
		/// </summary>
		/// <remarks>
		///     If set to <see langword="false" /> (default value), the base class implementation gets called and
		///     its return values are used as default values.
		///     <para />
		///     Sets the <see cref="MockBehavior.SkipBaseClass" /> to <paramref name="skipBaseClass" />.
		/// </remarks>
		public MockBehavior SkippingBaseClass(bool skipBaseClass = true)
			=> mockBehavior with
			{
				SkipBaseClass = skipBaseClass,
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

		/// <summary>
		///     Uses the given <paramref name="factory" /> to create default values for <typeparamref name="T" />.
		/// </summary>
		public MockBehavior WithDefaultValueFor<T>(Func<T> factory)
			=> mockBehavior.WithDefaultValueFor(new DefaultValueFactory(
				t => t == typeof(T),
				(_, _) => factory()));
	}
}
