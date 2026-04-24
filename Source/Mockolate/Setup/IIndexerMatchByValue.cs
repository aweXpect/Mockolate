namespace Mockolate.Setup;

/// <summary>
///     Closure-free match contract for a single-argument indexer getter.
/// </summary>
/// <remarks>
///     Implemented by <see cref="IndexerSetup{TValue, T1}" /> so the member-id indexed lookup can
///     evaluate a match without allocating a predicate closure.
/// </remarks>
public interface IIndexerGetterMatchByValue<in T1>
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches a getter invocation with
	///     argument <paramref name="p1" />.
	/// </summary>
	bool Matches(T1 p1);
}

/// <summary>
///     Closure-free match contract for a two-argument indexer getter.
/// </summary>
public interface IIndexerGetterMatchByValue<in T1, in T2>
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches a getter invocation with the
	///     given arguments.
	/// </summary>
	bool Matches(T1 p1, T2 p2);
}

/// <summary>
///     Closure-free match contract for a three-argument indexer getter.
/// </summary>
public interface IIndexerGetterMatchByValue<in T1, in T2, in T3>
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches a getter invocation with the
	///     given arguments.
	/// </summary>
	bool Matches(T1 p1, T2 p2, T3 p3);
}

/// <summary>
///     Closure-free match contract for a four-argument indexer getter.
/// </summary>
public interface IIndexerGetterMatchByValue<in T1, in T2, in T3, in T4>
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches a getter invocation with the
	///     given arguments.
	/// </summary>
	bool Matches(T1 p1, T2 p2, T3 p3, T4 p4);
}

/// <summary>
///     Closure-free match contract for a single-argument indexer setter. The trailing
///     <typeparamref name="TValue" /> is the value being assigned.
/// </summary>
public interface IIndexerSetterMatchByValue<in T1, in TValue>
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches a setter invocation with the
	///     given argument and assigned value.
	/// </summary>
	bool Matches(T1 p1, TValue value);
}

/// <summary>
///     Closure-free match contract for a two-argument indexer setter.
/// </summary>
public interface IIndexerSetterMatchByValue<in T1, in T2, in TValue>
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches a setter invocation with the
	///     given arguments and assigned value.
	/// </summary>
	bool Matches(T1 p1, T2 p2, TValue value);
}

/// <summary>
///     Closure-free match contract for a three-argument indexer setter.
/// </summary>
public interface IIndexerSetterMatchByValue<in T1, in T2, in T3, in TValue>
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches a setter invocation with the
	///     given arguments and assigned value.
	/// </summary>
	bool Matches(T1 p1, T2 p2, T3 p3, TValue value);
}

/// <summary>
///     Closure-free match contract for a four-argument indexer setter.
/// </summary>
public interface IIndexerSetterMatchByValue<in T1, in T2, in T3, in T4, in TValue>
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches a setter invocation with the
	///     given arguments and assigned value.
	/// </summary>
	bool Matches(T1 p1, T2 p2, T3 p3, T4 p4, TValue value);
}
