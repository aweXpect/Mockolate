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
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly ScenarioState _scenarioState;

	/// <inheritdoc cref="MockRegistry" />
	public MockRegistry(MockBehavior behavior, object?[]? constructorParameters = null)
	{
		Behavior = behavior;
		ConstructorParameters = constructorParameters;
		Interactions = new MockInteractions { RecordingEnabled = !behavior.SkipInteractionRecording };
		Setup = new MockSetups();
		_scenarioState = new ScenarioState();
		Wraps = null;
	}

	/// <inheritdoc cref="MockRegistry" />
	public MockRegistry(MockRegistry registry, object wraps)
	{
		Behavior = registry.Behavior;
		ConstructorParameters = registry.ConstructorParameters;
		Interactions = new MockInteractions { RecordingEnabled = !registry.Behavior.SkipInteractionRecording };
		Setup = registry.Setup;
		_scenarioState = registry._scenarioState;
		Wraps = wraps;
	}

	/// <inheritdoc cref="MockRegistry" />
	public MockRegistry(MockRegistry registry, object?[] constructorParameters)
	{
		Behavior = registry.Behavior;
		ConstructorParameters = constructorParameters;
		Interactions = registry.Interactions;
		Setup = registry.Setup;
		_scenarioState = registry._scenarioState;
		Wraps = registry.Wraps;
	}

	/// <inheritdoc cref="MockRegistry" />
	public MockRegistry(MockRegistry registry, MockInteractions interactions)
	{
		Behavior = registry.Behavior;
		ConstructorParameters = registry.ConstructorParameters;
		Interactions = interactions;
		Setup = registry.Setup;
		_scenarioState = registry._scenarioState;
		Wraps = registry.Wraps;
	}

	/// <summary>
	///     The current scenario of the mock. Use <see cref="TransitionTo(string)" /> to change the active scenario.
	/// </summary>
	public string Scenario => _scenarioState.Current;

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
	///     Transitions the mock to the given <paramref name="scenario" />.
	/// </summary>
	/// <param name="scenario">The name of the scenario to activate. Use <see cref="string.Empty" /> for the default scope.</param>
	public void TransitionTo(string scenario)
		=> _scenarioState.Current = scenario;

	/// <summary>
	///     Implicitly converts a <see cref="MockBehavior" /> to a <see cref="MockRegistry" /> with the given behavior and an
	///     empty interaction collection.
	/// </summary>
	public static implicit operator MockRegistry(MockBehavior behavior)
	{
		return new MockRegistry(behavior);
	}

	private sealed class ScenarioState
	{
		public string Current { get; set; } = "";
	}
}
