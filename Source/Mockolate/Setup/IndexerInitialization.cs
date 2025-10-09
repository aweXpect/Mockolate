using System;

namespace Mockolate.Setup;

/// <summary>
///     Sets up indexers on the mock.
/// </summary>
public class IndexerInitialization(IMockSetup mockSetup)
{
	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IndexerInitialization InitializeWith<TValue, T1>(T1 parameter, TValue value)
	{
		mockSetup.SetIndexerValue([parameter], value);
		return this;
	}
	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IndexerInitialization InitializeWith<TValue, T1, T2>(T1 p1, T2 p2, TValue value)
	{
		mockSetup.SetIndexerValue([p1, p2], value);
		return this;
	}
	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IndexerInitialization InitializeWith<TValue, T1, T2, T3>(T1 p1, T2 p2, T3 p3, TValue value)
	{
		mockSetup.SetIndexerValue([p1, p2, p3], value);
		return this;
	}
}
