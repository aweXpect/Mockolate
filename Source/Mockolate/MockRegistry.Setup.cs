using Mockolate.Setup;

namespace Mockolate;

public partial class MockRegistry
{
	/// <summary>
	///     The registered setups for the mock, including methods, properties, indexers and events.
	/// </summary>
	internal MockSetups Setup { get; }

	/// <summary>
	///     Registers <paramref name="indexerSetup" /> for the default scenario.
	/// </summary>
	/// <param name="indexerSetup">The indexer setup produced by the fluent <c>Setup[...]</c> API.</param>
	public void SetupIndexer(IndexerSetup indexerSetup)
		=> SetupIndexer("", indexerSetup);

	/// <summary>
	///     Registers <paramref name="indexerSetup" /> for the given <paramref name="scenario" />.
	/// </summary>
	/// <param name="scenario">The scenario name the setup applies to. The setup only applies while <see cref="Scenario" /> equals <paramref name="scenario" /> and does not leak into the default bucket.</param>
	/// <param name="indexerSetup">The indexer setup produced by the fluent <c>Setup[...]</c> API.</param>
	public void SetupIndexer(string scenario, IndexerSetup indexerSetup)
		=> Setup.GetOrCreateScenario(scenario).Indexers.Add(indexerSetup);

	/// <summary>
	///     Registers <paramref name="methodSetup" /> for the default scenario.
	/// </summary>
	/// <param name="methodSetup">The method setup produced by the fluent <c>Setup.MethodName(...)</c> API.</param>
	public void SetupMethod(MethodSetup methodSetup)
		=> SetupMethod("", methodSetup);

	/// <summary>
	///     Registers <paramref name="methodSetup" /> for the given <paramref name="scenario" />.
	/// </summary>
	/// <param name="scenario">The scenario name the setup applies to. The setup only applies while <see cref="Scenario" /> equals <paramref name="scenario" /> and does not leak into the default bucket.</param>
	/// <param name="methodSetup">The method setup produced by the fluent <c>Setup.MethodName(...)</c> API.</param>
	public void SetupMethod(string scenario, MethodSetup methodSetup)
		=> Setup.GetOrCreateScenario(scenario).Methods.Add(methodSetup);

	/// <summary>
	///     Registers <paramref name="propertySetup" /> for the default scenario.
	/// </summary>
	/// <param name="propertySetup">The property setup produced by the fluent <c>Setup.PropertyName</c> API.</param>
	public void SetupProperty(PropertySetup propertySetup)
		=> SetupProperty("", propertySetup);

	/// <summary>
	///     Registers <paramref name="propertySetup" /> for the given <paramref name="scenario" />.
	/// </summary>
	/// <param name="scenario">The scenario name the setup applies to. The setup only applies while <see cref="Scenario" /> equals <paramref name="scenario" /> and does not leak into the default bucket.</param>
	/// <param name="propertySetup">The property setup produced by the fluent <c>Setup.PropertyName</c> API.</param>
	public void SetupProperty(string scenario, PropertySetup propertySetup)
		=> Setup.GetOrCreateScenario(scenario).Properties.Add(propertySetup);

	/// <summary>
	///     Registers <paramref name="eventSetup" /> for the default scenario.
	/// </summary>
	/// <param name="eventSetup">The event setup produced by the fluent <c>Setup.EventName</c> API.</param>
	public void SetupEvent(EventSetup eventSetup)
		=> SetupEvent("", eventSetup);

	/// <summary>
	///     Registers <paramref name="eventSetup" /> for the given <paramref name="scenario" />.
	/// </summary>
	/// <param name="scenario">The scenario name the setup applies to. The setup only applies while <see cref="Scenario" /> equals <paramref name="scenario" /> and does not leak into the default bucket.</param>
	/// <param name="eventSetup">The event setup produced by the fluent <c>Setup.EventName</c> API.</param>
	public void SetupEvent(string scenario, EventSetup eventSetup)
		=> Setup.GetOrCreateScenario(scenario).Events.Add(eventSetup);
}
