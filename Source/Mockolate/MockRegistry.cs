using System.Diagnostics;
using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate;

/// <summary>
///     Registry for mock behavior, setups and interactions.
/// </summary>
/// <remarks>
///     It also gives access to constructor parameters and the wrapped instance.
/// </remarks>
[DebuggerDisplay("{Interactions} | {Setup}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public partial class MockRegistry
{
	/// <inheritdoc cref="MockRegistry" />
	public MockRegistry(MockBehavior behavior, object?[]? constructorParameters = null)
	{
		Behavior = behavior;
		ConstructorParameters = constructorParameters;
		Interactions = new MockInteractions();
		Setup = new MockSetups();
		Wraps = null;
	}

	/// <inheritdoc cref="MockRegistry" />
	public MockRegistry(MockRegistry registry, object wraps)
	{
		Behavior = registry.Behavior;
		ConstructorParameters = registry.ConstructorParameters;
		Interactions = new MockInteractions();
		Setup = registry.Setup;
		Wraps = wraps;
	}

	/// <inheritdoc cref="MockRegistry" />
	public MockRegistry(MockRegistry registry, object?[] constructorParameters)
	{
		Behavior = registry.Behavior;
		ConstructorParameters = constructorParameters;
		Interactions = registry.Interactions;
		Setup = registry.Setup;
		Wraps = registry.Wraps;
	}

	/// <inheritdoc cref="MockRegistry" />
	public MockRegistry(MockRegistry registry, MockInteractions interactions)
	{
		Behavior = registry.Behavior;
		ConstructorParameters = registry.ConstructorParameters;
		Interactions = interactions;
		Setup = registry.Setup;
		Wraps = registry.Wraps;
	}

	/// <summary>
	///     The current scenario of the mock.
	/// </summary>
	public string Scenario { get; set; } = "";

	/// <summary>
	///     Gets the behavior settings used by this mock instance.
	/// </summary>
	public MockBehavior Behavior { get; }

	/// <summary>
	///     The constructor parameters used to create the base class.
	/// </summary>
	public object?[]? ConstructorParameters { get; }

	/// <summary>
	///     The instance the mock wraps.
	/// </summary>
	public object? Wraps { get; }

	/// <summary>
	///     Implicitly converts a <see cref="MockBehavior" /> to a <see cref="MockRegistry" /> with the given behavior and an
	///     empty interaction collection.
	/// </summary>
	public static implicit operator MockRegistry(MockBehavior behavior)
	{
		return new MockRegistry(behavior);
	}
}
