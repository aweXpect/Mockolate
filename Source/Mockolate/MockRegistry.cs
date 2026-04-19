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

	/// <summary>
	///     Creates a new <see cref="MockRegistry" /> with the given <paramref name="behavior" /> and, optionally,
	///     <paramref name="constructorParameters" /> for a class mock's base-class constructor.
	/// </summary>
	/// <param name="behavior">The <see cref="MockBehavior" /> that governs how the mock responds without a matching setup.</param>
	/// <param name="constructorParameters">Values forwarded to the base-class constructor, or <see langword="null" /> if no base constructor call is needed.</param>
	public MockRegistry(MockBehavior behavior, object?[]? constructorParameters = null)
	{
		Behavior = behavior;
		ConstructorParameters = constructorParameters;
		Interactions = new MockInteractions { SkipInteractionRecording = behavior.SkipInteractionRecording };
		Setup = new MockSetups();
		_scenarioState = new ScenarioState();
		Wraps = null;
	}

	/// <summary>
	///     Creates a <see cref="MockRegistry" /> that shares setup and scenario state with <paramref name="registry" />
	///     but records interactions on a fresh bucket and forwards calls to <paramref name="wraps" />.
	/// </summary>
	/// <param name="registry">The source registry whose <see cref="Behavior" />, setups, and scenario state are reused.</param>
	/// <param name="wraps">The real instance that the mock should delegate calls to. See <see cref="Wraps" />.</param>
	public MockRegistry(MockRegistry registry, object wraps)
	{
		Behavior = registry.Behavior;
		ConstructorParameters = registry.ConstructorParameters;
		Interactions = new MockInteractions { SkipInteractionRecording = registry.Behavior.SkipInteractionRecording };
		Setup = registry.Setup;
		_scenarioState = registry._scenarioState;
		Wraps = wraps;
	}

	/// <summary>
	///     Creates a <see cref="MockRegistry" /> that shares all state with <paramref name="registry" /> but exposes
	///     a different set of <paramref name="constructorParameters" /> to the base-class constructor.
	/// </summary>
	/// <param name="registry">The source registry whose setups, interactions, scenario state, and wrapped instance are reused.</param>
	/// <param name="constructorParameters">Values forwarded to the base-class constructor of the new mock instance.</param>
	public MockRegistry(MockRegistry registry, object?[] constructorParameters)
	{
		Behavior = registry.Behavior;
		ConstructorParameters = constructorParameters;
		Interactions = registry.Interactions;
		Setup = registry.Setup;
		_scenarioState = registry._scenarioState;
		Wraps = registry.Wraps;
	}

	/// <summary>
	///     Creates a <see cref="MockRegistry" /> that shares all state with <paramref name="registry" /> but records
	///     into the supplied <paramref name="interactions" /> collection. Used for scoped monitoring.
	/// </summary>
	/// <param name="registry">The source registry whose behavior, setups, constructor parameters, scenario state, and wrapped instance are reused.</param>
	/// <param name="interactions">The interaction collection that new invocations should be appended to.</param>
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
	///     The name of the currently active scenario. Defaults to <see cref="string.Empty" />. Use
	///     <see cref="TransitionTo(string)" /> or the generator-emitted <c>TransitionTo</c> chained onto a setup to
	///     change it.
	/// </summary>
	/// <remarks>
	///     When a member is invoked, Mockolate resolves a matching setup in this order:
	///     <list type="number">
	///         <item><description>the active scenario's bucket, when <see cref="Scenario" /> is not empty;</description></item>
	///         <item><description>the default bucket (setups registered via <c>sut.Mock.Setup.*</c>);</description></item>
	///         <item><description>the default response determined by <see cref="Behavior" />.</description></item>
	///     </list>
	///     Scenario setups add to, rather than replace, the default bucket - register catch-alls at the default scope
	///     and override specific members per scenario.
	/// </remarks>
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
	///     The real instance that public, non-protected calls are forwarded to, or <see langword="null" /> when the
	///     mock is a plain (non-wrapping) mock.
	/// </summary>
	/// <remarks>
	///     Populated by the generator-emitted <c>Wrapping(instance)</c> extension. Public members delegate to
	///     <see cref="Wraps" /> unless a matching setup overrides them; protected members still go through the base
	///     class implementation. All forwarded calls are recorded on <see cref="Interactions" /> and can be verified
	///     like any other interaction.
	/// </remarks>
	public object? Wraps { get; }

	/// <summary>
	///     Transitions the mock to the given <paramref name="scenario" />, so that subsequent member invocations
	///     look up setups in that scenario's bucket first, and fall back to the default bucket if nothing matches.
	/// </summary>
	/// <param name="scenario">The name of the scenario to activate. Use <see cref="string.Empty" /> for the default scope.</param>
	/// <remarks>
	///     Transitioning to a scenario name for which no setups were registered via <c>InScenario(name)</c> is
	///     legal - resolution will simply fall straight through to the default bucket. See <see cref="Scenario" />
	///     for the full resolution order.
	/// </remarks>
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
