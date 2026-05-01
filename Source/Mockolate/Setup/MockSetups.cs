using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace Mockolate.Setup;

[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
internal partial class MockSetups : MockScenarioSetup
{
	internal Dictionary<string, MockScenarioSetup>? Scenarios { get; private set; }

	public MockScenarioSetup GetOrCreateScenario(string setupScenario)
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

	public bool TryGetScenario(string setupScenario, [NotNullWhen(true)] out MockScenarioSetup? scenario)
	{
		if (string.IsNullOrEmpty(setupScenario))
		{
			scenario = this;
			return true;
		}

		if (Scenarios is not null && Scenarios.TryGetValue(setupScenario, out scenario))
		{
			return true;
		}

		scenario = null;
		return false;
	}
}

internal class MockScenarioSetup
{
	private MockSetups.EventSetups? _events;
	private MockSetups.IndexerSetups? _indexers;
	private MockSetups.MethodSetups? _methods;
	private MockSetups.PropertySetups? _properties;

	internal MockSetups.EventSetups Events
	{
		get
		{
			if (_events is null)
			{
				Interlocked.CompareExchange(ref _events, new MockSetups.EventSetups(), null);
			}

			return _events!;
		}
	}

	internal MockSetups.IndexerSetups Indexers
	{
		get
		{
			if (_indexers is null)
			{
				Interlocked.CompareExchange(ref _indexers, new MockSetups.IndexerSetups(), null);
			}

			return _indexers!;
		}
	}

	internal MockSetups.MethodSetups Methods
	{
		get
		{
			if (_methods is null)
			{
				Interlocked.CompareExchange(ref _methods, new MockSetups.MethodSetups(), null);
			}

			return _methods!;
		}
	}

	internal MockSetups.PropertySetups Properties
	{
		get
		{
			if (_properties is null)
			{
				Interlocked.CompareExchange(ref _properties, new MockSetups.PropertySetups(), null);
			}

			return _properties!;
		}
	}

	/// <inheritdoc cref="object.ToString()" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		StringBuilder sb = new();
		int methodCount = _methods?.Count ?? 0;
		if (methodCount > 0)
		{
			sb.Append(methodCount).Append(methodCount == 1 ? " method, " : " methods, ");
		}

		int propertyCount = _properties?.Count ?? 0;
		if (propertyCount > 0)
		{
			sb.Append(propertyCount).Append(propertyCount == 1 ? " property, " : " properties, ");
		}

		int indexerCount = _indexers?.Count ?? 0;
		if (indexerCount > 0)
		{
			sb.Append(indexerCount).Append(indexerCount == 1 ? " indexer, " : " indexers, ");
		}

		int eventCount = _events?.Count ?? 0;
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
