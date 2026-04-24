namespace Mockolate.Setup;

/// <summary>
///     Marker for a method setup that accepts no parameters. Implemented by
///     <see cref="ReturnMethodSetup{TReturn}" /> and <see cref="VoidMethodSetup" /> so the member-id
///     indexed lookup can match without a closure.
/// </summary>
public interface IMethodMatchByValue
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches a parameterless invocation.
	/// </summary>
	bool Matches();
}

/// <summary>
///     Closure-free match contract for a single-parameter method setup.
/// </summary>
public interface IMethodMatchByValue<in T1>
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches an invocation with argument
	///     <paramref name="p1" />.
	/// </summary>
	bool Matches(T1 p1);
}

/// <summary>
///     Closure-free match contract for a two-parameter method setup.
/// </summary>
public interface IMethodMatchByValue<in T1, in T2>
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches an invocation with the given
	///     arguments.
	/// </summary>
	bool Matches(T1 p1, T2 p2);
}

/// <summary>
///     Closure-free match contract for a three-parameter method setup.
/// </summary>
public interface IMethodMatchByValue<in T1, in T2, in T3>
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches an invocation with the given
	///     arguments.
	/// </summary>
	bool Matches(T1 p1, T2 p2, T3 p3);
}

/// <summary>
///     Closure-free match contract for a four-parameter method setup.
/// </summary>
public interface IMethodMatchByValue<in T1, in T2, in T3, in T4>
{
	/// <summary>
	///     Returns <see langword="true" /> when this setup matches an invocation with the given
	///     arguments.
	/// </summary>
	bool Matches(T1 p1, T2 p2, T3 p3, T4 p4);
}
