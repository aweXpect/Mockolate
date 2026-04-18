using System;

namespace Mockolate;

/// <summary>
///     Extension methods for <see cref="MockBehavior" />.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
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
		///     Specifies whether interaction recording should be skipped.
		/// </summary>
		/// <remarks>
		///     If set to <see langword="false" /> (default value), every interaction with the mock is recorded
		///     and can be verified later.
		///     <para />
		///     Skipping interaction recording avoids the per-call allocation of an interaction record, the
		///     interaction-list lock, and the <see cref="Mockolate.Interactions.MockInteractions" />
		///     added-event. Setups, returns, callbacks, and base-class delegation continue to work normally -
		///     only verification is disabled. Any attempt to verify throws a
		///     <see cref="Mockolate.Exceptions.MockException" />.
		///     <para />
		///     Sets the <see cref="MockBehavior.SkipInteractionRecording" /> to <paramref name="skipInteractionRecording" />.
		/// </remarks>
		public MockBehavior SkippingInteractionRecording(bool skipInteractionRecording = true)
			=> mockBehavior with
			{
				SkipInteractionRecording = skipInteractionRecording,
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
