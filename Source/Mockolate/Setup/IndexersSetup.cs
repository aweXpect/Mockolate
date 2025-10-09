using System;

namespace Mockolate.Setup;

/// <summary>
///     Sets up indexers on the mock.
/// </summary>
public class IndexersSetup(IMockSetup mockSetup)
{
	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IndexersSetup InitializeWith<TValue, T1>(T1 parameter, TValue value)
	{
		mockSetup.SetIndexerValue([parameter], value);
		return this;
	}
	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IndexersSetup InitializeWith<TValue, T1, T2>(T1 p1, T2 p2, TValue value)
	{
		mockSetup.SetIndexerValue([p1, p2], value);
		return this;
	}
	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IndexersSetup InitializeWith<TValue, T1, T2, T3>(T1 p1, T2 p2, T3 p3, TValue value)
	{
		mockSetup.SetIndexerValue([p1, p2, p3], value);
		return this;
	}

	/// <summary>
	///     Sets up the indexer for the <paramref name="parameter"/> with a custom <paramref name="setup" />.
	/// </summary>
	public IndexersSetup For<T1>(
		With.Parameter<T1> parameter,
		Func<IndexerSetup<T1>, IndexerSetup<T1>> setup)
	{
		var indexerSetup = new IndexerSetup<T1>(parameter);
		mockSetup.RegisterIndexer(setup(indexerSetup));
		return this;
	}

	/// <summary>
	///     Sets up the indexer for <paramref name="parameter1"/> and <paramref name="parameter2"/> with a custom <paramref name="setup" />.
	/// </summary>
	public IndexersSetup For<T1, T2>(
		With.Parameter<T1> parameter1,
		With.Parameter<T2> parameter2,
		Func<IndexerSetup<T1, T2>, IndexerSetup<T1, T2>> setup)
	{
		var indexerSetup = new IndexerSetup<T1, T2>(parameter1, parameter2);
		mockSetup.RegisterIndexer(setup(indexerSetup));
		return this;
	}

	/// <summary>
	///     Sets up the indexer for <paramref name="parameter1"/>, <paramref name="parameter2"/> and <paramref name="parameter3"/> with a custom <paramref name="setup" />.
	/// </summary>
	public IndexersSetup For<T1, T2, T3>(
		With.Parameter<T1> parameter1,
		With.Parameter<T2> parameter2,
		With.Parameter<T3> parameter3,
		Func<IndexerSetup<T1, T2, T3>, IndexerSetup<T1, T2, T3>> setup)
	{
		var indexerSetup = new IndexerSetup<T1, T2, T3>(parameter1, parameter2, parameter3);
		mockSetup.RegisterIndexer(setup(indexerSetup));
		return this;
	}
}
