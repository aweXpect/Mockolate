using Mockolate.Setup;

namespace Mockolate;

public partial class MockRegistry
{
	/// <summary>
	///     The registered setups for the mock, including methods, properties, indexers and events.
	/// </summary>
	internal MockSetups Setup { get; }

	/// <summary>
	///     Registers the <paramref name="indexerSetup" /> in the mock.
	/// </summary>
	public void SetupIndexer(IndexerSetup indexerSetup)
		=> Setup.GetScenario(SetupScenario).Indexers.Add(indexerSetup);

	/// <summary>
	///     Registers the <paramref name="methodSetup" /> in the mock.
	/// </summary>
	public void SetupMethod(MethodSetup methodSetup)
		=> Setup.GetScenario(SetupScenario).Methods.Add(methodSetup);

	/// <summary>
	///     Registers the <paramref name="propertySetup" /> in the mock.
	/// </summary>
	public void SetupProperty(PropertySetup propertySetup)
		=> Setup.GetScenario(SetupScenario).Properties.Add(propertySetup);

	/// <summary>
	///     Registers the <paramref name="eventSetup" /> in the mock.
	/// </summary>
	public void SetupEvent(EventSetup eventSetup)
		=> Setup.GetScenario(SetupScenario).Events.Add(eventSetup);
}
