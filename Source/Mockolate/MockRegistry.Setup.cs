using Mockolate.Setup;

namespace Mockolate;

public partial class MockRegistry
{
	/// <summary>
	///     The registered setups for the mock, including methods, properties, indexers and events.
	/// </summary>
	internal MockSetups Setup { get; }

	/// <summary>
	///     Registers the <paramref name="indexerSetup" /> in the mock for the given <paramref name="scenario" />.
	/// </summary>
	public void SetupIndexer(string scenario, IndexerSetup indexerSetup)
		=> Setup.GetScenario(scenario).Indexers.Add(indexerSetup);

	/// <summary>
	///     Registers the <paramref name="methodSetup" /> in the mock for the given <paramref name="scenario" />.
	/// </summary>
	public void SetupMethod(string scenario, MethodSetup methodSetup)
		=> Setup.GetScenario(scenario).Methods.Add(methodSetup);

	/// <summary>
	///     Registers the <paramref name="propertySetup" /> in the mock for the given <paramref name="scenario" />.
	/// </summary>
	public void SetupProperty(string scenario, PropertySetup propertySetup)
	{
		propertySetup.MockRegistry = this;
		Setup.GetScenario(scenario).Properties.Add(propertySetup);
	}

	/// <summary>
	///     Registers the <paramref name="eventSetup" /> in the mock for the given <paramref name="scenario" />.
	/// </summary>
	public void SetupEvent(string scenario, EventSetup eventSetup)
		=> Setup.GetScenario(scenario).Events.Add(eventSetup);

	/// <summary>
	///     Registers the <paramref name="indexerSetup" /> in the mock.
	/// </summary>
	public void SetupIndexer(IndexerSetup indexerSetup)
		=> SetupIndexer("", indexerSetup);

	/// <summary>
	///     Registers the <paramref name="methodSetup" /> in the mock.
	/// </summary>
	public void SetupMethod(MethodSetup methodSetup)
		=> SetupMethod("", methodSetup);

	/// <summary>
	///     Registers the <paramref name="propertySetup" /> in the mock.
	/// </summary>
	public void SetupProperty(PropertySetup propertySetup)
		=> SetupProperty("", propertySetup);

	/// <summary>
	///     Registers the <paramref name="eventSetup" /> in the mock.
	/// </summary>
	public void SetupEvent(EventSetup eventSetup)
		=> SetupEvent("", eventSetup);
}
