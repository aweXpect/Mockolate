using System.Diagnostics;
using Mockolate.Interactions;

namespace Mockolate;

/// <summary>
///     The registration class for mocks.
/// </summary>
[DebuggerDisplay("{Interactions} | {Setup}")]
[DebuggerNonUserCode]
public partial class MockRegistry
{
	/// <inheritdoc cref="MockRegistry" />
	public MockRegistry(MockBehavior behavior)
	{
		Behavior = behavior;
		Interactions = new MockInteractions();
	}

	/// <inheritdoc cref="MockRegistry" />
	public MockRegistry(MockBehavior behavior, MockInteractions interactions)
	{
		Behavior = behavior;
		Interactions = interactions;
	}

	/// <summary>
	///     Gets the behavior settings used by this mock instance.
	/// </summary>
	public MockBehavior Behavior { get; }

	/// <summary>
	///     Implicitly converts a <see cref="MockBehavior" /> to a <see cref="MockRegistry" /> with the given behavior and an empty interaction collection.
	/// </summary>
	public static implicit operator MockRegistry(MockBehavior behavior)
	{
		return new MockRegistry(behavior);
	}
}
