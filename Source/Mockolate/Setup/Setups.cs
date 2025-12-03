using System.Collections.Generic;

namespace Mockolate.Setup;

/// <summary>
///     A collection of setups.
/// </summary>
public class Setups(
	IReadOnlyList<IndexerSetup> indexers,
	IReadOnlyCollection<PropertySetup> properties,
	IReadOnlyCollection<MethodSetup> methods)
{
	/// <summary>
	///     The indexer setups.
	/// </summary>
	public IReadOnlyCollection<IndexerSetup> Indexers => indexers;

	/// <summary>
	///     The property setups.
	/// </summary>
	public IReadOnlyCollection<PropertySetup> Properties => properties;

	/// <summary>
	///     The method setups.
	/// </summary>
	public IReadOnlyCollection<MethodSetup> Methods => methods;
}
