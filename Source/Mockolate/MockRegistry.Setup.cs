using System;
using System.Diagnostics;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate;

public partial class MockRegistry
{
	/// <summary>
	///     The registered setups for the mock, including methods, properties, indexers and events.
	/// </summary>
	internal MockSetups Setup { get; }

	/// <summary>
	///     Retrieves the latest method setup that matches the specified <paramref name="methodInvocation" />,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private MethodSetup? GetMethodSetup(MethodInvocation methodInvocation)
	{
		return Setup.Methods.GetLatestOrDefault(Predicate);

		[DebuggerNonUserCode]
		bool Predicate(MethodSetup setup)
		{
			return ((IInteractiveMethodSetup)setup).Matches(methodInvocation);
		}
	}

	/// <summary>
	///     Retrieves the latest indexer setup that matches the specified <paramref name="interaction" />,
	///     or returns <see langword="null" /> if no matching setup is found.
	/// </summary>
	private IndexerSetup? GetIndexerSetup(IndexerAccess interaction)
		=> Setup.Indexers.GetLatestOrDefault(setup => ((IInteractiveIndexerSetup)setup).Matches(interaction));

	/// <summary>
	///     Gets the indexer value for the given <paramref name="parameters" />.
	/// </summary>
	private TValue GetIndexerValue<TValue>(IInteractiveIndexerSetup? setup, Func<TValue> defaultValueGenerator,
		NamedParameterValue[] parameters)
		=> Setup.Indexers.GetOrAddValue(parameters, defaultValueGenerator);

	/// <summary>
	///     Registers the <paramref name="indexerSetup" /> in the mock.
	/// </summary>
	public void SetupIndexer(IndexerSetup indexerSetup) => Setup.Indexers.Add(indexerSetup);

	/// <summary>
	///     Registers the <paramref name="methodSetup" /> in the mock.
	/// </summary>
	public void SetupMethod(MethodSetup methodSetup) => Setup.Methods.Add(methodSetup);

	/// <summary>
	///     Registers the <paramref name="propertySetup" /> in the mock.
	/// </summary>
	public void SetupProperty(PropertySetup propertySetup)
		=> Setup.Properties.Add(propertySetup);
}
