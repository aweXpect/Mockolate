using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Mockolate.Setup;

[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
internal partial class MockSetups : MockScenarioSetup
{
	internal Dictionary<string, MockScenarioSetup>? Scenarios { get; private set; }

	public MockScenarioSetup GetScenario(string setupScenario)
	{
		if (string.IsNullOrEmpty(setupScenario))
		{
			return this;
		}

		Scenarios ??= new Dictionary<string, MockScenarioSetup>();
		if (Scenarios.TryGetValue(setupScenario, out MockScenarioSetup? scenario))
		{
			return scenario;
		}

		scenario = new MockScenarioSetup();
		Scenarios.Add(setupScenario, scenario);
		return scenario;
	}
}

internal class MockScenarioSetup
{
	internal MockSetups.EventSetups Events { get; } = new();
	internal MockSetups.IndexerSetups Indexers { get; } = new();
	internal MockSetups.MethodSetups Methods { get; } = new();
	internal MockSetups.PropertySetups Properties { get; } = new();

	/// <inheritdoc cref="object.ToString()" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		StringBuilder sb = new();
		int methodCount = Methods.Count;
		if (methodCount > 0)
		{
			sb.Append(methodCount).Append(methodCount == 1 ? " method, " : " methods, ");
		}

		int propertyCount = Properties.Count;
		if (propertyCount > 0)
		{
			sb.Append(propertyCount).Append(propertyCount == 1 ? " property, " : " properties, ");
		}

		int indexerCount = Indexers.Count;
		if (indexerCount > 0)
		{
			sb.Append(indexerCount).Append(indexerCount == 1 ? " indexer, " : " indexers, ");
		}

		int eventCount = Events.Count;
		if (eventCount > 0)
		{
			sb.Append(eventCount).Append(eventCount == 1 ? " event, " : " events, ");
		}

		if (sb.Length == 0)
		{
			return "no setups";
		}

		sb.Length -= 2;
		return sb.ToString();
	}
}
