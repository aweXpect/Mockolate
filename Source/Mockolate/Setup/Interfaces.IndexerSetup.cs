using System;
using System.Diagnostics.CodeAnalysis;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     Interface for hiding some implementation details of <see cref="IndexerSetup" />.
/// </summary>
public interface IInteractiveIndexerSetup : ISetup
{
	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	bool? SkipBaseClass();

	/// <summary>
	///     Gets a value indicating whether this setup has return calls configured.
	/// </summary>
	bool HasReturnCalls();

	/// <summary>
	///     Checks if the <paramref name="indexerAccess" /> matches the setup.
	/// </summary>
	bool Matches(IndexerAccess indexerAccess);

	/// <summary>
	///     Attempts to retrieve the initial <paramref name="value" /> for the <paramref name="parameters" />, if an
	///     initialization is set up.
	/// </summary>
	void GetInitialValue<TValue>(MockBehavior behavior, Func<TValue> defaultValueGenerator, NamedParameterValue[] parameters,
		[NotNullWhen(true)] out TValue value);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer getter for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerGetterSetup<TValue, out T1>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	IIndexerSetupCallbackBuilder<TValue, T1> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameter of the indexer.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1> Do(Action<T1> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameter of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1> Do(Action<T1, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameter of the indexer and the value
	///     of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1> Do(Action<int, T1, TValue> callback);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer setter for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetterSetup<TValue, out T1>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	IIndexerSetupCallbackBuilder<TValue, T1> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the value the indexer is set to as single parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1> Do(Action<TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameter of the indexer and the value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1> Do(Action<T1, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameter of the indexer and the value
	///     the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1> Do(Action<int, T1, TValue> callback);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetup<TValue, out T1>
{
	/// <summary>
	///     Sets up callbacks on the getter.
	/// </summary>
	IIndexerGetterSetup<TValue, T1> OnGet { get; }

	/// <summary>
	///     Sets up callbacks on the setter.
	/// </summary>
	IIndexerSetterSetup<TValue, T1> OnSet { get; }

	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IIndexerSetup<TValue, T1> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	IIndexerSetup<TValue, T1> InitializeWith(TValue value);

	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	IIndexerSetup<TValue, T1> InitializeWith(Func<T1, TValue> valueGenerator);

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1> Returns(TValue returnValue);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1> Returns(Func<TValue> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameter of the indexer.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1> Returns(Func<T1, TValue> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameter of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1> Returns(Func<T1, TValue, TValue> callback);

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the indexer is read.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1> Throws<TException>() where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the indexer is read.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the indexer is read.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the indexer is read.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameter of the indexer.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1> Throws(Func<T1, Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the indexer is read.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameter of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1> Throws(Func<T1, TValue, Exception> callback);
}

/// <summary>
///     Sets up a callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetupCallbackBuilder<TValue, out T1>
	: IIndexerSetupCallbackWhenBuilder<TValue, T1>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IIndexerSetupCallbackBuilder<TValue, T1> InParallel();

	/// <summary>
	///     Limits the callback to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerSetupCallbackWhenBuilder<TValue, T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetupCallbackWhenBuilder<TValue, out T1>
	: IIndexerSetup<TValue, T1>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupCallbackBuilder{TValue, T1}.When(Func{int, bool})" /> evaluates to <see langword="true" />
	///     ).
	/// </remarks>
	IIndexerSetupCallbackWhenBuilder<TValue, T1> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupCallbackBuilder{TValue, T1}.When(Func{int, bool})" /> evaluates to <see langword="true" />
	///     ).
	/// </remarks>
	IIndexerSetup<TValue, T1> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetupReturnBuilder<TValue, out T1>
	: IIndexerSetupReturnWhenBuilder<TValue, T1>
{
	/// <summary>
	///     Limits the return/throw to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerSetupReturnWhenBuilder<TValue, T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetupReturnWhenBuilder<TValue, out T1>
	: IIndexerSetup<TValue, T1>
{
	/// <summary>
	///     Repeats the return/throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupReturnBuilder{TValue, T1}.When(Func{int, bool})" />).
	/// </remarks>
	IIndexerSetupReturnWhenBuilder<TValue, T1> For(int times);

	/// <summary>
	///     Deactivates the return/throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions
	///     (<see cref="IIndexerSetupReturnBuilder{TValue, T1}.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IIndexerSetup<TValue, T1> Only(int times);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer getter for <typeparamref name="T1" /> and
///     <typeparamref name="T2" />.
/// </summary>
public interface IIndexerGetterSetup<TValue, out T1, out T2>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	IIndexerSetupCallbackBuilder<TValue, T1, T2> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2> Do(Action<T1, T2> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2> Do(Action<T1, T2, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the
	///     value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2> Do(Action<int, T1, T2, TValue> callback);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer setter for <typeparamref name="T1" /> and
///     <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetterSetup<TValue, out T1, out T2>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	IIndexerSetupCallbackBuilder<TValue, T1, T2> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the value the indexer is set to as single parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2> Do(Action<TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2> Do(Action<T1, T2, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the
	///     value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2> Do(Action<int, T1, T2, TValue> callback);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetup<TValue, out T1, out T2>
{
	/// <summary>
	///     Sets up callbacks on the getter.
	/// </summary>
	IIndexerGetterSetup<TValue, T1, T2> OnGet { get; }

	/// <summary>
	///     Sets up callbacks on the setter.
	/// </summary>
	IIndexerSetterSetup<TValue, T1, T2> OnSet { get; }

	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IIndexerSetup<TValue, T1, T2> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	IIndexerSetup<TValue, T1, T2> InitializeWith(TValue value);

	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	IIndexerSetup<TValue, T1, T2> InitializeWith(Func<T1, T2, TValue> valueGenerator);

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(TValue returnValue);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(Func<TValue> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(Func<T1, T2, TValue> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(Func<T1, T2, TValue, TValue> callback);

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the indexer is read.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2> Throws<TException>() where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the indexer is read.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the indexer is read.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the indexer is read.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1, T2> Throws(Func<T1, T2, Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the indexer is read.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1, T2> Throws(Func<T1, T2, TValue, Exception> callback);
}

/// <summary>
///     Sets up a callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and
///     <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetupCallbackBuilder<TValue, out T1, out T2>
	: IIndexerSetupCallbackWhenBuilder<TValue, T1, T2>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IIndexerSetupCallbackBuilder<TValue, T1, T2> InParallel();

	/// <summary>
	///     Limits the callback to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and
///     <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetupCallbackWhenBuilder<TValue, out T1, out T2>
	: IIndexerSetup<TValue, T1, T2>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupCallbackBuilder{TValue, T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupCallbackBuilder{TValue, T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetup<TValue, T1, T2> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and
///     <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetupReturnBuilder<TValue, out T1, out T2>
	: IIndexerSetupReturnWhenBuilder<TValue, T1, T2>
{
	/// <summary>
	///     Limits the return/throw to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" /> and
///     <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetupReturnWhenBuilder<TValue, out T1, out T2>
	: IIndexerSetup<TValue, T1, T2>
{
	/// <summary>
	///     Repeats the return/throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions
	///     (<see cref="IIndexerSetupReturnBuilder{TValue, T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2> For(int times);

	/// <summary>
	///     Deactivates the return/throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupReturnBuilder{TValue, T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetup<TValue, T1, T2> Only(int times);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer getter for <typeparamref name="T1" />, <typeparamref name="T2" />
///     and
///     <typeparamref name="T3" />.
/// </summary>
public interface IIndexerGetterSetup<TValue, out T1, out T2, out T3>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<T1, T2, T3> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<T1, T2, T3, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the
	///     value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<int, T1, T2, T3, TValue> callback);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer setter for <typeparamref name="T1" />, <typeparamref name="T2" />
///     and
///     <typeparamref name="T3" />.
/// </summary>
public interface IIndexerSetterSetup<TValue, out T1, out T2, out T3>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the value the indexer is set to as single parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<T1, T2, T3, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the
	///     value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<int, T1, T2, T3, TValue> callback);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />, <typeparamref name="T2" /> and
///     <typeparamref name="T3" />.
/// </summary>
public interface IIndexerSetup<TValue, out T1, out T2, out T3>
{
	/// <summary>
	///     Sets up callbacks on the getter.
	/// </summary>
	IIndexerGetterSetup<TValue, T1, T2, T3> OnGet { get; }

	/// <summary>
	///     Sets up callbacks on the setter.
	/// </summary>
	IIndexerSetterSetup<TValue, T1, T2, T3> OnSet { get; }

	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IIndexerSetup<TValue, T1, T2, T3> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	IIndexerSetup<TValue, T1, T2, T3> InitializeWith(TValue value);

	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	IIndexerSetup<TValue, T1, T2, T3> InitializeWith(Func<T1, T2, T3, TValue> valueGenerator);

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(TValue returnValue);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<TValue> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<T1, T2, T3, TValue> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<T1, T2, T3, TValue, TValue> callback);

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the indexer is read.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws<TException>() where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the indexer is read.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the indexer is read.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the indexer is read.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws(Func<T1, T2, T3, Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the indexer is read.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws(Func<T1, T2, T3, TValue, Exception> callback);
}

/// <summary>
///     Sets up a callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerSetupCallbackBuilder<TValue, out T1, out T2, out T3>
	: IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> InParallel();

	/// <summary>
	///     Limits the callback to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerSetupCallbackWhenBuilder<TValue, out T1, out T2, out T3>
	: IIndexerSetup<TValue, T1, T2, T3>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupCallbackBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupCallbackBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetup<TValue, T1, T2, T3> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerSetupReturnBuilder<TValue, out T1, out T2, out T3>
	: IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3>
{
	/// <summary>
	///     Limits the return/throw to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerSetupReturnWhenBuilder<TValue, out T1, out T2, out T3>
	: IIndexerSetup<TValue, T1, T2, T3>
{
	/// <summary>
	///     Repeats the return/throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupReturnBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3> For(int times);

	/// <summary>
	///     Deactivates the return/throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupReturnBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetup<TValue, T1, T2, T3> Only(int times);
}

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer getter for <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface IIndexerGetterSetup<TValue, out T1, out T2, out T3, out T4>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<T1, T2, T3, T4> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<T1, T2, T3, T4, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the
	///     value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<int, T1, T2, T3, T4, TValue> callback);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer setter for <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface IIndexerSetterSetup<TValue, out T1, out T2, out T3, out T4>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the value the indexer is set to as single parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<T1, T2, T3, T4, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the
	///     value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<int, T1, T2, T3, T4, TValue> callback);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface IIndexerSetup<TValue, out T1, out T2, out T3, out T4>
{
	/// <summary>
	///     Sets up callbacks on the getter.
	/// </summary>
	IIndexerGetterSetup<TValue, T1, T2, T3, T4> OnGet { get; }

	/// <summary>
	///     Sets up callbacks on the setter.
	/// </summary>
	IIndexerSetterSetup<TValue, T1, T2, T3, T4> OnSet { get; }

	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IIndexerSetup<TValue, T1, T2, T3, T4> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	IIndexerSetup<TValue, T1, T2, T3, T4> InitializeWith(TValue value);

	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	IIndexerSetup<TValue, T1, T2, T3, T4> InitializeWith(Func<T1, T2, T3, T4, TValue> valueGenerator);

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(TValue returnValue);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<TValue> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<T1, T2, T3, T4, TValue> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<T1, T2, T3, T4, TValue, TValue> callback);

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the indexer is read.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws<TException>() where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the indexer is read.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the indexer is read.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the indexer is read.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the indexer is read.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, TValue, Exception> callback);
}

/// <summary>
///     Sets up a callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface
	IIndexerSetupCallbackBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> InParallel();

	/// <summary>
	///     Limits the callback to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface IIndexerSetupCallbackWhenBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerSetup<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupCallbackBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupCallbackBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetup<TValue, T1, T2, T3, T4> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface IIndexerSetupReturnBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Limits the return/throw to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface IIndexerSetupReturnWhenBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerSetup<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Repeats the return/throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupReturnBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3, T4> For(int times);

	/// <summary>
	///     Deactivates the return/throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetupReturnBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetup<TValue, T1, T2, T3, T4> Only(int times);
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
