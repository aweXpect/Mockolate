using System;

namespace Mockolate.Setup;

/// <summary>
///     Sets up indexers of type <typeparamref name="TValue"/> with one parameter of type <typeparamref name="T1" />.
/// </summary>
public class IndexersSetup<TValue, T1>(IMockSetup mockSetup)
{
	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IndexersSetup<TValue, T1> InitializeWith(T1 parameter, TValue value)
	{
		mockSetup.SetIndexerValue([parameter], value);
		return this;
	}

	/// <summary>
	///     Sets up the indexer for <paramref name="p1"/> with a custom <paramref name="setup" />.
	/// </summary>
	public IndexersSetup<TValue, T1> For(With.Parameter<T1> p1, Func<IndexerSetup<TValue, T1>, IndexerSetup<TValue, T1>> setup)
	{
		var indexerSetup = new IndexerSetup<TValue, T1>(p1);
		mockSetup.RegisterIndexer(setup(indexerSetup));
		return this;
	}
}

/// <summary>
///     Sets up indexers of type <typeparamref name="TValue"/> with two parameters of type <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
public class IndexersSetup<TValue, T1, T2>(IMockSetup mockSetup)
{
	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IndexersSetup<TValue, T1, T2> InitializeWith(T1 p1, T2 p2, TValue value)
	{
		mockSetup.SetIndexerValue([p1, p2], value);
		return this;
	}

	/// <summary>
	///     Sets up the indexer for <paramref name="p1"/> and <paramref name="p2"/> with a custom <paramref name="setup" />.
	/// </summary>
	public IndexersSetup<TValue, T1, T2> For(With.Parameter<T1> p1, With.Parameter<T2> p2, Func<IndexerSetup, IndexerSetup> setup)
	{
		var indexerSetup = new IndexerSetup<TValue, T1, T2>(p1, p2);
		mockSetup.RegisterIndexer(setup(indexerSetup));
		return this;
	}
}

/// <summary>
///     Sets up indexers of type <typeparamref name="TValue"/> with three parameters of type <typeparamref name="T1" />, <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public class IndexersSetup<TValue, T1, T2, T3>(IMockSetup mockSetup)
{
	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IndexersSetup<TValue, T1, T2, T3> InitializeWith(T1 p1, T2 p2, T3 p3, TValue value)
	{
		mockSetup.SetIndexerValue([p1, p2, p3], value);
		return this;
	}

	/// <summary>
	///     Sets up the indexer for <paramref name="p1"/>, <paramref name="p2"/> and <paramref name="p3"/> with a custom <paramref name="setup" />.
	/// </summary>
	public IndexersSetup<TValue, T1, T2, T3> For(With.Parameter<T1> p1, With.Parameter<T2> p2, With.Parameter<T3> p3, Func<IndexerSetup, IndexerSetup> setup)
	{
		var indexerSetup = new IndexerSetup<TValue, T1, T2, T3>(p1, p2, p3);
		mockSetup.RegisterIndexer(setup(indexerSetup));
		return this;
	}
}
