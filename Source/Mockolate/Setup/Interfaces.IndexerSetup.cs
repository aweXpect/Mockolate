using System;
using Mockolate.Interactions;

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
	///     Checks if the <paramref name="indexerAccess" /> matches the setup.
	/// </summary>
	bool Matches(IndexerAccess indexerAccess);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer getter for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerGetterSetup<TValue, out T1>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	IIndexerGetterSetupCallbackBuilder<TValue, T1> Do(Action callback);

	/// <summary>
	///     Transitions the scenario to the given <paramref name="scenario" /> whenever the indexer is read.
	/// </summary>
	/// <param name="scenario">The name of the new scenario.</param>
	IIndexerGetterSetupParallelCallbackBuilder<TValue, T1> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer getter for <typeparamref name="T1" /> with callback support for the parameter.
/// </summary>
public interface IIndexerGetterSetupWithCallback<TValue, out T1> : IIndexerGetterSetup<TValue, T1>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameter of the indexer.
	/// </remarks>
	IIndexerGetterSetupCallbackBuilder<TValue, T1> Do(Action<T1> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameter of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerGetterSetupCallbackBuilder<TValue, T1> Do(Action<T1, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameter of the indexer and the value
	///     of the indexer as last parameter.
	/// </remarks>
	IIndexerGetterSetupCallbackBuilder<TValue, T1> Do(Action<int, T1, TValue> callback);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer setter for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetterSetup<TValue, out T1>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	IIndexerSetterSetupCallbackBuilder<TValue, T1> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the value the indexer is set to as single parameter.
	/// </remarks>
	IIndexerSetterSetupCallbackBuilder<TValue, T1> Do(Action<TValue> callback);

	/// <summary>
	///     Transitions the scenario to the given <paramref name="scenario" /> whenever the indexer is written to.
	/// </summary>
	/// <param name="scenario">The name of the new scenario.</param>
	IIndexerSetterSetupParallelCallbackBuilder<TValue, T1> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer setter for <typeparamref name="T1" /> with callback support for the parameter.
/// </summary>
public interface IIndexerSetterSetupWithCallback<TValue, out T1> : IIndexerSetterSetup<TValue, T1>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameter of the indexer and the value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetterSetupCallbackBuilder<TValue, T1> Do(Action<T1, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameter of the indexer and the value
	///     the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetterSetupCallbackBuilder<TValue, T1> Do(Action<int, T1, TValue> callback);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetup<TValue, out T1>
{
	/// <summary>
	///     Sets up callbacks on the getter.
	/// </summary>
	IIndexerGetterSetupWithCallback<TValue, T1> OnGet { get; }

	/// <summary>
	///     Sets up callbacks on the setter.
	/// </summary>
	IIndexerSetterSetupWithCallback<TValue, T1> OnSet { get; }

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
	///     Registers the <paramref name="returnValue" /> for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1> Returns(TValue returnValue);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1> Returns(Func<TValue> callback);

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
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> with callback support for the parameter.
/// </summary>
public interface IIndexerSetupWithCallback<TValue, out T1> : IIndexerSetup<TValue, T1>
{
	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	IIndexerSetup<TValue, T1> InitializeWith(Func<T1, TValue> valueGenerator);

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
public interface IIndexerGetterSetupCallbackBuilder<TValue, out T1>
	: IIndexerGetterSetupParallelCallbackBuilder<TValue, T1>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IIndexerGetterSetupParallelCallbackBuilder<TValue, T1> InParallel();
}

/// <summary>
///     Sets up a parallel callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerGetterSetupParallelCallbackBuilder<TValue, out T1>
	: IIndexerGetterSetupCallbackWhenBuilder<TValue, T1>
{
	/// <summary>
	///     Limits the callback to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerGetterSetupCallbackWhenBuilder<TValue, T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerGetterSetupCallbackWhenBuilder<TValue, out T1>
	: IIndexerSetupWithCallback<TValue, T1>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerGetterSetupParallelCallbackBuilder{TValue, T1}.When(Func{int, bool})" /> evaluates to <see langword="true" />
	///     ).
	/// </remarks>
	IIndexerGetterSetupCallbackWhenBuilder<TValue, T1> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerGetterSetupParallelCallbackBuilder{TValue, T1}.When(Func{int, bool})" /> evaluates to <see langword="true" />
	///     ).
	/// </remarks>
	IIndexerSetup<TValue, T1> Only(int times);
}

/// <summary>
///     Sets up a setter callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetterSetupCallbackBuilder<TValue, out T1>
	: IIndexerSetterSetupParallelCallbackBuilder<TValue, T1>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IIndexerSetterSetupParallelCallbackBuilder<TValue, T1> InParallel();
}

/// <summary>
///     Sets up a parallel setter callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetterSetupParallelCallbackBuilder<TValue, out T1>
	: IIndexerSetterSetupCallbackWhenBuilder<TValue, T1>
{
	/// <summary>
	///     Limits the callback to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerSetterSetupCallbackWhenBuilder<TValue, T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when setter callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetterSetupCallbackWhenBuilder<TValue, out T1>
	: IIndexerSetupWithCallback<TValue, T1>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetterSetupParallelCallbackBuilder{TValue, T1}.When(Func{int, bool})" /> evaluates to <see langword="true" />
	///     ).
	/// </remarks>
	IIndexerSetterSetupCallbackWhenBuilder<TValue, T1> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetterSetupParallelCallbackBuilder{TValue, T1}.When(Func{int, bool})" /> evaluates to <see langword="true" />
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
	: IIndexerSetupWithCallback<TValue, T1>
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
	IIndexerGetterSetupCallbackBuilder<TValue, T1, T2> Do(Action callback);

	/// <summary>
	///     Transitions the scenario to the given <paramref name="scenario" /> whenever the indexer is read.
	/// </summary>
	/// <param name="scenario">The name of the new scenario.</param>
	IIndexerGetterSetupParallelCallbackBuilder<TValue, T1, T2> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer getter for <typeparamref name="T1" /> and
///     <typeparamref name="T2" /> with callback support for the parameters.
/// </summary>
public interface IIndexerGetterSetupWithCallback<TValue, out T1, out T2> : IIndexerGetterSetup<TValue, T1, T2>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer.
	/// </remarks>
	IIndexerGetterSetupCallbackBuilder<TValue, T1, T2> Do(Action<T1, T2> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerGetterSetupCallbackBuilder<TValue, T1, T2> Do(Action<T1, T2, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the
	///     value of the indexer as last parameter.
	/// </remarks>
	IIndexerGetterSetupCallbackBuilder<TValue, T1, T2> Do(Action<int, T1, T2, TValue> callback);
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
	IIndexerSetterSetupCallbackBuilder<TValue, T1, T2> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the value the indexer is set to as single parameter.
	/// </remarks>
	IIndexerSetterSetupCallbackBuilder<TValue, T1, T2> Do(Action<TValue> callback);

	/// <summary>
	///     Transitions the scenario to the given <paramref name="scenario" /> whenever the indexer is written to.
	/// </summary>
	/// <param name="scenario">The name of the new scenario.</param>
	IIndexerSetterSetupParallelCallbackBuilder<TValue, T1, T2> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer setter for <typeparamref name="T1" /> and
///     <typeparamref name="T2" /> with callback support for the parameters.
/// </summary>
public interface IIndexerSetterSetupWithCallback<TValue, out T1, out T2> : IIndexerSetterSetup<TValue, T1, T2>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetterSetupCallbackBuilder<TValue, T1, T2> Do(Action<T1, T2, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the
	///     value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetterSetupCallbackBuilder<TValue, T1, T2> Do(Action<int, T1, T2, TValue> callback);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetup<TValue, out T1, out T2>
{
	/// <summary>
	///     Sets up callbacks on the getter.
	/// </summary>
	IIndexerGetterSetupWithCallback<TValue, T1, T2> OnGet { get; }

	/// <summary>
	///     Sets up callbacks on the setter.
	/// </summary>
	IIndexerSetterSetupWithCallback<TValue, T1, T2> OnSet { get; }

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
	///     Registers the <paramref name="returnValue" /> for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(TValue returnValue);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(Func<TValue> callback);

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
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and <typeparamref name="T2" />
///     with callback support for the parameters.
/// </summary>
public interface IIndexerSetupWithCallback<TValue, out T1, out T2> : IIndexerSetup<TValue, T1, T2>
{
	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	IIndexerSetup<TValue, T1, T2> InitializeWith(Func<T1, T2, TValue> valueGenerator);

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
public interface IIndexerGetterSetupCallbackBuilder<TValue, out T1, out T2>
	: IIndexerGetterSetupParallelCallbackBuilder<TValue, T1, T2>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IIndexerGetterSetupParallelCallbackBuilder<TValue, T1, T2> InParallel();
}

/// <summary>
///     Sets up a parallel callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and
///     <typeparamref name="T2" />.
/// </summary>
public interface IIndexerGetterSetupParallelCallbackBuilder<TValue, out T1, out T2>
	: IIndexerGetterSetupCallbackWhenBuilder<TValue, T1, T2>
{
	/// <summary>
	///     Limits the callback to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerGetterSetupCallbackWhenBuilder<TValue, T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and
///     <typeparamref name="T2" />.
/// </summary>
public interface IIndexerGetterSetupCallbackWhenBuilder<TValue, out T1, out T2>
	: IIndexerSetupWithCallback<TValue, T1, T2>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerGetterSetupParallelCallbackBuilder{TValue, T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerGetterSetupCallbackWhenBuilder<TValue, T1, T2> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerGetterSetupParallelCallbackBuilder{TValue, T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetup<TValue, T1, T2> Only(int times);
}

/// <summary>
///     Sets up a setter callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and
///     <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetterSetupCallbackBuilder<TValue, out T1, out T2>
	: IIndexerSetterSetupParallelCallbackBuilder<TValue, T1, T2>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IIndexerSetterSetupParallelCallbackBuilder<TValue, T1, T2> InParallel();
}

/// <summary>
///     Sets up a parallel setter callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and
///     <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetterSetupParallelCallbackBuilder<TValue, out T1, out T2>
	: IIndexerSetterSetupCallbackWhenBuilder<TValue, T1, T2>
{
	/// <summary>
	///     Limits the callback to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerSetterSetupCallbackWhenBuilder<TValue, T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when setter callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and
///     <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetterSetupCallbackWhenBuilder<TValue, out T1, out T2>
	: IIndexerSetupWithCallback<TValue, T1, T2>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetterSetupParallelCallbackBuilder{TValue, T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetterSetupCallbackWhenBuilder<TValue, T1, T2> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetterSetupParallelCallbackBuilder{TValue, T1, T2}.When(Func{int, bool})" /> evaluates to
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
	: IIndexerSetupWithCallback<TValue, T1, T2>
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
	IIndexerGetterSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action callback);

	/// <summary>
	///     Transitions the scenario to the given <paramref name="scenario" /> whenever the indexer is read.
	/// </summary>
	/// <param name="scenario">The name of the new scenario.</param>
	IIndexerGetterSetupParallelCallbackBuilder<TValue, T1, T2, T3> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer getter for <typeparamref name="T1" />, <typeparamref name="T2" />
///     and <typeparamref name="T3" /> with callback support for the parameters.
/// </summary>
public interface IIndexerGetterSetupWithCallback<TValue, out T1, out T2, out T3> : IIndexerGetterSetup<TValue, T1, T2, T3>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer.
	/// </remarks>
	IIndexerGetterSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<T1, T2, T3> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerGetterSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<T1, T2, T3, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the
	///     value of the indexer as last parameter.
	/// </remarks>
	IIndexerGetterSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<int, T1, T2, T3, TValue> callback);
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
	IIndexerSetterSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the value the indexer is set to as single parameter.
	/// </remarks>
	IIndexerSetterSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<TValue> callback);

	/// <summary>
	///     Transitions the scenario to the given <paramref name="scenario" /> whenever the indexer is written to.
	/// </summary>
	/// <param name="scenario">The name of the new scenario.</param>
	IIndexerSetterSetupParallelCallbackBuilder<TValue, T1, T2, T3> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer setter for <typeparamref name="T1" />, <typeparamref name="T2" />
///     and <typeparamref name="T3" /> with callback support for the parameters.
/// </summary>
public interface IIndexerSetterSetupWithCallback<TValue, out T1, out T2, out T3> : IIndexerSetterSetup<TValue, T1, T2, T3>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetterSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<T1, T2, T3, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the
	///     value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetterSetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<int, T1, T2, T3, TValue> callback);
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
	IIndexerGetterSetupWithCallback<TValue, T1, T2, T3> OnGet { get; }

	/// <summary>
	///     Sets up callbacks on the setter.
	/// </summary>
	IIndexerSetterSetupWithCallback<TValue, T1, T2, T3> OnSet { get; }

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
	///     Registers the <paramref name="returnValue" /> for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(TValue returnValue);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<TValue> callback);

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
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />, <typeparamref name="T2" /> and
///     <typeparamref name="T3" /> with callback support for the parameters.
/// </summary>
public interface IIndexerSetupWithCallback<TValue, out T1, out T2, out T3> : IIndexerSetup<TValue, T1, T2, T3>
{
	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	IIndexerSetup<TValue, T1, T2, T3> InitializeWith(Func<T1, T2, T3, TValue> valueGenerator);

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
public interface IIndexerGetterSetupCallbackBuilder<TValue, out T1, out T2, out T3>
	: IIndexerGetterSetupParallelCallbackBuilder<TValue, T1, T2, T3>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IIndexerGetterSetupParallelCallbackBuilder<TValue, T1, T2, T3> InParallel();
}

/// <summary>
///     Sets up a parallel callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerGetterSetupParallelCallbackBuilder<TValue, out T1, out T2, out T3>
	: IIndexerGetterSetupCallbackWhenBuilder<TValue, T1, T2, T3>
{
	/// <summary>
	///     Limits the callback to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerGetterSetupCallbackWhenBuilder<TValue, T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerGetterSetupCallbackWhenBuilder<TValue, out T1, out T2, out T3>
	: IIndexerSetupWithCallback<TValue, T1, T2, T3>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerGetterSetupParallelCallbackBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IIndexerGetterSetupCallbackWhenBuilder<TValue, T1, T2, T3> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerGetterSetupParallelCallbackBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetup<TValue, T1, T2, T3> Only(int times);
}

/// <summary>
///     Sets up a setter callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerSetterSetupCallbackBuilder<TValue, out T1, out T2, out T3>
	: IIndexerSetterSetupParallelCallbackBuilder<TValue, T1, T2, T3>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IIndexerSetterSetupParallelCallbackBuilder<TValue, T1, T2, T3> InParallel();
}

/// <summary>
///     Sets up a parallel setter callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerSetterSetupParallelCallbackBuilder<TValue, out T1, out T2, out T3>
	: IIndexerSetterSetupCallbackWhenBuilder<TValue, T1, T2, T3>
{
	/// <summary>
	///     Limits the callback to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerSetterSetupCallbackWhenBuilder<TValue, T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when setter callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerSetterSetupCallbackWhenBuilder<TValue, out T1, out T2, out T3>
	: IIndexerSetupWithCallback<TValue, T1, T2, T3>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetterSetupParallelCallbackBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IIndexerSetterSetupCallbackWhenBuilder<TValue, T1, T2, T3> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetterSetupParallelCallbackBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" /> evaluates to
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
	: IIndexerSetupWithCallback<TValue, T1, T2, T3>
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
	IIndexerGetterSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action callback);

	/// <summary>
	///     Transitions the scenario to the given <paramref name="scenario" /> whenever the indexer is read.
	/// </summary>
	/// <param name="scenario">The name of the new scenario.</param>
	IIndexerGetterSetupParallelCallbackBuilder<TValue, T1, T2, T3, T4> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer getter for <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" /> with callback support for the parameters.
/// </summary>
public interface IIndexerGetterSetupWithCallback<TValue, out T1, out T2, out T3, out T4> : IIndexerGetterSetup<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer.
	/// </remarks>
	IIndexerGetterSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<T1, T2, T3, T4> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value of the indexer as last parameter.
	/// </remarks>
	IIndexerGetterSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<T1, T2, T3, T4, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the
	///     value of the indexer as last parameter.
	/// </remarks>
	IIndexerGetterSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<int, T1, T2, T3, T4, TValue> callback);
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
	IIndexerSetterSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the value the indexer is set to as single parameter.
	/// </remarks>
	IIndexerSetterSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<TValue> callback);

	/// <summary>
	///     Transitions the scenario to the given <paramref name="scenario" /> whenever the indexer is written to.
	/// </summary>
	/// <param name="scenario">The name of the new scenario.</param>
	IIndexerSetterSetupParallelCallbackBuilder<TValue, T1, T2, T3, T4> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer setter for <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" /> with callback support for the parameters.
/// </summary>
public interface IIndexerSetterSetupWithCallback<TValue, out T1, out T2, out T3, out T4> : IIndexerSetterSetup<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the parameters of the indexer and the value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetterSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<T1, T2, T3, T4, TValue> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter, the parameters of the indexer and the
	///     value the indexer is set to as last parameter.
	/// </remarks>
	IIndexerSetterSetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<int, T1, T2, T3, T4, TValue> callback);
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
	IIndexerGetterSetupWithCallback<TValue, T1, T2, T3, T4> OnGet { get; }

	/// <summary>
	///     Sets up callbacks on the setter.
	/// </summary>
	IIndexerSetterSetupWithCallback<TValue, T1, T2, T3, T4> OnSet { get; }

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
	///     Registers the <paramref name="returnValue" /> for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(TValue returnValue);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this indexer.
	/// </summary>
	IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<TValue> callback);

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
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" /> with callback support for the parameters.
/// </summary>
public interface IIndexerSetupWithCallback<TValue, out T1, out T2, out T3, out T4> : IIndexerSetup<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	IIndexerSetup<TValue, T1, T2, T3, T4> InitializeWith(Func<T1, T2, T3, T4, TValue> valueGenerator);

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
	IIndexerGetterSetupCallbackBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerGetterSetupParallelCallbackBuilder<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IIndexerGetterSetupParallelCallbackBuilder<TValue, T1, T2, T3, T4> InParallel();
}

/// <summary>
///     Sets up a parallel callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface
	IIndexerGetterSetupParallelCallbackBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerGetterSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Limits the callback to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerGetterSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface IIndexerGetterSetupCallbackWhenBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerSetupWithCallback<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerGetterSetupParallelCallbackBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerGetterSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerGetterSetupParallelCallbackBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetup<TValue, T1, T2, T3, T4> Only(int times);
}

/// <summary>
///     Sets up a setter callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface
	IIndexerSetterSetupCallbackBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerSetterSetupParallelCallbackBuilder<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IIndexerSetterSetupParallelCallbackBuilder<TValue, T1, T2, T3, T4> InParallel();
}

/// <summary>
///     Sets up a parallel setter callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface
	IIndexerSetterSetupParallelCallbackBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerSetterSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Limits the callback to only execute for indexer accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the indexer has been accessed so far.
	/// </remarks>
	IIndexerSetterSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when setter callback for a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface IIndexerSetterSetupCallbackWhenBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerSetupWithCallback<TValue, T1, T2, T3, T4>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetterSetupParallelCallbackBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IIndexerSetterSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IIndexerSetterSetupParallelCallbackBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
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
	: IIndexerSetupWithCallback<TValue, T1, T2, T3, T4>
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

/// <summary>
///     Setup for a mocked <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> that the mock only
///     reads.
/// </summary>
/// <remarks>
///     Used instead of <see cref="IIndexerSetup{TValue, T1}" /> when the mock has no setter to intercept, either
///     because the indexer is declared without one or because its setter is not accessible from the mock's assembly.
///     Writes then never reach the mock, so <see cref="IIndexerSetup{TValue, T1}.OnSet" /> is not offered.
/// </remarks>
public interface IIndexerGetterOnlySetup<TValue, out T1>
{
	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.OnGet" />
	IIndexerGetterOnlyGetterSetup<TValue, T1> OnGet { get; }

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.SkippingBaseClass(bool)" />
	IIndexerGetterOnlySetup<TValue, T1> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.InitializeWith(TValue)" />
	/// <remarks>
	///     Seeds the value that reads return. Unlike a read-write indexer there is no setter to update the
	///     slot afterwards, so it stays at <paramref name="value" /> unless a <c>Returns</c> entry applies.
	/// </remarks>
	IIndexerGetterOnlySetup<TValue, T1> InitializeWith(TValue value);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1}.InitializeWith(Func{T1, TValue})" />
	IIndexerGetterOnlySetup<TValue, T1> InitializeWith(Func<T1, TValue> valueGenerator);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Returns(TValue)" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1> Returns(TValue returnValue);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Returns(Func{TValue})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1> Returns(Func<TValue> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1}.Returns(Func{T1, TValue})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1> Returns(Func<T1, TValue> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1}.Returns(Func{T1, TValue, TValue})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1> Returns(Func<T1, TValue, TValue> callback);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Throws{TException}()" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Throws(Exception)" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1> Throws(Exception exception);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Throws(Func{Exception})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1> Throws(Func<Exception> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1}.Throws(Func{T1, Exception})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1> Throws(Func<T1, Exception> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1}.Throws(Func{T1, TValue, Exception})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1> Throws(Func<T1, TValue, Exception> callback);
}

/// <summary>
///     Setup for a mocked <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> that the mock only
///     writes.
/// </summary>
/// <remarks>
///     The write-only counterpart of <see cref="IIndexerGetterOnlySetup{TValue, T1}" />: the mock has no getter to
///     intercept, so <see cref="IIndexerSetup{TValue, T1}.OnGet" />, <c>InitializeWith</c> and the
///     <c>Returns</c>/<c>Throws</c> read-sequence are not offered.
/// </remarks>
public interface IIndexerSetterOnlySetup<TValue, out T1>
{
	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.OnSet" />
	IIndexerSetterOnlySetterSetup<TValue, T1> OnSet { get; }

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.SkippingBaseClass(bool)" />
	IIndexerSetterOnlySetup<TValue, T1> SkippingBaseClass(bool skipBaseClass = true);
}

/// <summary>
///     Setup for attaching side-effects to the getter of a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />.
/// </summary>
/// <remarks>
///     The counterpart of <see cref="IIndexerGetterSetupWithCallback{TValue, T1}" /> for
///     <see cref="IIndexerGetterOnlySetup{TValue, T1}" />: the returned builders stay on the getter-only surface,
///     so chaining can never reach <see cref="IIndexerSetup{TValue, T1}.OnSet" />.
/// </remarks>
public interface IIndexerGetterOnlyGetterSetup<TValue, out T1>
{
	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1}.Do(Action)" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1> Do(Action callback);

	/// <inheritdoc cref="IIndexerGetterSetupWithCallback{TValue, T1}.Do(Action{T1})" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1> Do(Action<T1> callback);

	/// <inheritdoc cref="IIndexerGetterSetupWithCallback{TValue, T1}.Do(Action{T1, TValue})" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1> Do(Action<T1, TValue> callback);

	/// <inheritdoc cref="IIndexerGetterSetupWithCallback{TValue, T1}.Do(Action{int, T1, TValue})" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1> Do(Action<int, T1, TValue> callback);

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1}.TransitionTo(string)" />
	IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, T1> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a callback for a get-only <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerGetterOnlySetupCallbackBuilder<TValue, out T1>
	: IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, T1>
{
	/// <inheritdoc cref="IIndexerGetterSetupCallbackBuilder{TValue, T1}.InParallel()" />
	IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, T1> InParallel();
}

/// <summary>
///     Sets up a parallel callback for a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />.
/// </summary>
public interface IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, out T1>
	: IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, T1>
{
	/// <inheritdoc cref="IIndexerGetterSetupParallelCallbackBuilder{TValue, T1}.When(Func{int, bool})" />
	IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a get-only <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, out T1>
	: IIndexerGetterOnlySetup<TValue, T1>
{
	/// <inheritdoc cref="IIndexerGetterSetupCallbackWhenBuilder{TValue, T1}.For(int)" />
	IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, T1> For(int times);

	/// <inheritdoc cref="IIndexerGetterSetupCallbackWhenBuilder{TValue, T1}.Only(int)" />
	IIndexerGetterOnlySetup<TValue, T1> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />.
/// </summary>
public interface IIndexerGetterOnlySetupReturnBuilder<TValue, out T1>
	: IIndexerGetterOnlySetupReturnWhenBuilder<TValue, T1>
{
	/// <inheritdoc cref="IIndexerSetupReturnBuilder{TValue, T1}.When(Func{int, bool})" />
	IIndexerGetterOnlySetupReturnWhenBuilder<TValue, T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />.
/// </summary>
public interface IIndexerGetterOnlySetupReturnWhenBuilder<TValue, out T1>
	: IIndexerGetterOnlySetup<TValue, T1>
{
	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue, T1}.For(int)" />
	IIndexerGetterOnlySetupReturnWhenBuilder<TValue, T1> For(int times);

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue, T1}.Only(int)" />
	IIndexerGetterOnlySetup<TValue, T1> Only(int times);
}

/// <summary>
///     Setup for attaching side-effects to the setter of a set-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />.
/// </summary>
/// <remarks>
///     The counterpart of <see cref="IIndexerSetterSetupWithCallback{TValue, T1}" /> for
///     <see cref="IIndexerSetterOnlySetup{TValue, T1}" />: the returned builders stay on the setter-only surface,
///     so chaining can never reach <see cref="IIndexerSetup{TValue, T1}.OnGet" /> or the
///     <c>Returns</c>/<c>Throws</c> read-sequence.
/// </remarks>
public interface IIndexerSetterOnlySetterSetup<TValue, out T1>
{
	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1}.Do(Action)" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1> Do(Action callback);

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1}.Do(Action{TValue})" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1> Do(Action<TValue> callback);

	/// <inheritdoc cref="IIndexerSetterSetupWithCallback{TValue, T1}.Do(Action{T1, TValue})" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1> Do(Action<T1, TValue> callback);

	/// <inheritdoc cref="IIndexerSetterSetupWithCallback{TValue, T1}.Do(Action{int, T1, TValue})" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1> Do(Action<int, T1, TValue> callback);

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1}.TransitionTo(string)" />
	IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, T1> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a setter callback for a set-only <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetterOnlySetupCallbackBuilder<TValue, out T1>
	: IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, T1>
{
	/// <inheritdoc cref="IIndexerSetterSetupCallbackBuilder{TValue, T1}.InParallel()" />
	IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, T1> InParallel();
}

/// <summary>
///     Sets up a parallel setter callback for a set-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, out T1>
	: IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, T1>
{
	/// <inheritdoc cref="IIndexerSetterSetupParallelCallbackBuilder{TValue, T1}.When(Func{int, bool})" />
	IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when setter callback for a set-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />.
/// </summary>
public interface IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, out T1>
	: IIndexerSetterOnlySetup<TValue, T1>
{
	/// <inheritdoc cref="IIndexerSetterSetupCallbackWhenBuilder{TValue, T1}.For(int)" />
	IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, T1> For(int times);

	/// <inheritdoc cref="IIndexerSetterSetupCallbackWhenBuilder{TValue, T1}.Only(int)" />
	IIndexerSetterOnlySetup<TValue, T1> Only(int times);
}

/// <summary>
///     Setup for a mocked <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and
///     <typeparamref name="T2" /> that the mock only reads.
/// </summary>
/// <remarks>
///     Used instead of <see cref="IIndexerSetup{TValue, T1, T2}" /> when the mock has no setter to intercept, either
///     because the indexer is declared without one or because its setter is not accessible from the mock's assembly.
///     Writes then never reach the mock, so <see cref="IIndexerSetup{TValue, T1, T2}.OnSet" /> is not offered.
/// </remarks>
public interface IIndexerGetterOnlySetup<TValue, out T1, out T2>
{
	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.OnGet" />
	IIndexerGetterOnlyGetterSetup<TValue, T1, T2> OnGet { get; }

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.SkippingBaseClass(bool)" />
	IIndexerGetterOnlySetup<TValue, T1, T2> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.InitializeWith(TValue)" />
	/// <remarks>
	///     Seeds the value that reads return. Unlike a read-write indexer there is no setter to update the
	///     slot afterwards, so it stays at <paramref name="value" /> unless a <c>Returns</c> entry applies.
	/// </remarks>
	IIndexerGetterOnlySetup<TValue, T1, T2> InitializeWith(TValue value);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2}.InitializeWith(Func{T1, T2, TValue})" />
	IIndexerGetterOnlySetup<TValue, T1, T2> InitializeWith(Func<T1, T2, TValue> valueGenerator);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Returns(TValue)" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2> Returns(TValue returnValue);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Returns(Func{TValue})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2> Returns(Func<TValue> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2}.Returns(Func{T1, T2, TValue})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2> Returns(Func<T1, T2, TValue> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2}.Returns(Func{T1, T2, TValue, TValue})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2> Returns(Func<T1, T2, TValue, TValue> callback);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Throws{TException}()" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Throws(Exception)" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2> Throws(Exception exception);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Throws(Func{Exception})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2> Throws(Func<Exception> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2}.Throws(Func{T1, T2, Exception})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2> Throws(Func<T1, T2, Exception> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2}.Throws(Func{T1, T2, TValue, Exception})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2> Throws(Func<T1, T2, TValue, Exception> callback);
}

/// <summary>
///     Setup for a mocked <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and
///     <typeparamref name="T2" /> that the mock only writes.
/// </summary>
/// <remarks>
///     The write-only counterpart of <see cref="IIndexerGetterOnlySetup{TValue, T1, T2}" />: the mock has no getter
///     to intercept, so <see cref="IIndexerSetup{TValue, T1, T2}.OnGet" />, <c>InitializeWith</c> and the
///     <c>Returns</c>/<c>Throws</c> read-sequence are not offered.
/// </remarks>
public interface IIndexerSetterOnlySetup<TValue, out T1, out T2>
{
	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.OnSet" />
	IIndexerSetterOnlySetterSetup<TValue, T1, T2> OnSet { get; }

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.SkippingBaseClass(bool)" />
	IIndexerSetterOnlySetup<TValue, T1, T2> SkippingBaseClass(bool skipBaseClass = true);
}

/// <summary>
///     Setup for attaching side-effects to the getter of a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
/// <remarks>
///     The counterpart of <see cref="IIndexerGetterSetupWithCallback{TValue, T1, T2}" /> for
///     <see cref="IIndexerGetterOnlySetup{TValue, T1, T2}" />: the returned builders stay on the getter-only surface,
///     so chaining can never reach <see cref="IIndexerSetup{TValue, T1, T2}.OnSet" />.
/// </remarks>
public interface IIndexerGetterOnlyGetterSetup<TValue, out T1, out T2>
{
	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2}.Do(Action)" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1, T2> Do(Action callback);

	/// <inheritdoc cref="IIndexerGetterSetupWithCallback{TValue, T1, T2}.Do(Action{T1, T2})" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1, T2> Do(Action<T1, T2> callback);

	/// <inheritdoc cref="IIndexerGetterSetupWithCallback{TValue, T1, T2}.Do(Action{T1, T2, TValue})" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1, T2> Do(Action<T1, T2, TValue> callback);

	/// <inheritdoc cref="IIndexerGetterSetupWithCallback{TValue, T1, T2}.Do(Action{int, T1, T2, TValue})" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1, T2> Do(Action<int, T1, T2, TValue> callback);

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2}.TransitionTo(string)" />
	IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, T1, T2> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a callback for a get-only <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and
///     <typeparamref name="T2" />.
/// </summary>
public interface IIndexerGetterOnlySetupCallbackBuilder<TValue, out T1, out T2>
	: IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, T1, T2>
{
	/// <inheritdoc cref="IIndexerGetterSetupCallbackBuilder{TValue, T1, T2}.InParallel()" />
	IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, T1, T2> InParallel();
}

/// <summary>
///     Sets up a parallel callback for a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
public interface IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, out T1, out T2>
	: IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, T1, T2>
{
	/// <inheritdoc cref="IIndexerGetterSetupParallelCallbackBuilder{TValue, T1, T2}.When(Func{int, bool})" />
	IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a get-only <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />
///     and <typeparamref name="T2" />.
/// </summary>
public interface IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, out T1, out T2>
	: IIndexerGetterOnlySetup<TValue, T1, T2>
{
	/// <inheritdoc cref="IIndexerGetterSetupCallbackWhenBuilder{TValue, T1, T2}.For(int)" />
	IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, T1, T2> For(int times);

	/// <inheritdoc cref="IIndexerGetterSetupCallbackWhenBuilder{TValue, T1, T2}.Only(int)" />
	IIndexerGetterOnlySetup<TValue, T1, T2> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
public interface IIndexerGetterOnlySetupReturnBuilder<TValue, out T1, out T2>
	: IIndexerGetterOnlySetupReturnWhenBuilder<TValue, T1, T2>
{
	/// <inheritdoc cref="IIndexerSetupReturnBuilder{TValue, T1, T2}.When(Func{int, bool})" />
	IIndexerGetterOnlySetupReturnWhenBuilder<TValue, T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
public interface IIndexerGetterOnlySetupReturnWhenBuilder<TValue, out T1, out T2>
	: IIndexerGetterOnlySetup<TValue, T1, T2>
{
	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue, T1, T2}.For(int)" />
	IIndexerGetterOnlySetupReturnWhenBuilder<TValue, T1, T2> For(int times);

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue, T1, T2}.Only(int)" />
	IIndexerGetterOnlySetup<TValue, T1, T2> Only(int times);
}

/// <summary>
///     Setup for attaching side-effects to the setter of a set-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
/// <remarks>
///     The counterpart of <see cref="IIndexerSetterSetupWithCallback{TValue, T1, T2}" /> for
///     <see cref="IIndexerSetterOnlySetup{TValue, T1, T2}" />: the returned builders stay on the setter-only surface,
///     so chaining can never reach <see cref="IIndexerSetup{TValue, T1, T2}.OnGet" /> or the
///     <c>Returns</c>/<c>Throws</c> read-sequence.
/// </remarks>
public interface IIndexerSetterOnlySetterSetup<TValue, out T1, out T2>
{
	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2}.Do(Action)" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1, T2> Do(Action callback);

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2}.Do(Action{TValue})" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1, T2> Do(Action<TValue> callback);

	/// <inheritdoc cref="IIndexerSetterSetupWithCallback{TValue, T1, T2}.Do(Action{T1, T2, TValue})" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1, T2> Do(Action<T1, T2, TValue> callback);

	/// <inheritdoc cref="IIndexerSetterSetupWithCallback{TValue, T1, T2}.Do(Action{int, T1, T2, TValue})" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1, T2> Do(Action<int, T1, T2, TValue> callback);

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2}.TransitionTo(string)" />
	IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, T1, T2> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a setter callback for a set-only <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />
///     and <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetterOnlySetupCallbackBuilder<TValue, out T1, out T2>
	: IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, T1, T2>
{
	/// <inheritdoc cref="IIndexerSetterSetupCallbackBuilder{TValue, T1, T2}.InParallel()" />
	IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, T1, T2> InParallel();
}

/// <summary>
///     Sets up a parallel setter callback for a set-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, out T1, out T2>
	: IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, T1, T2>
{
	/// <inheritdoc cref="IIndexerSetterSetupParallelCallbackBuilder{TValue, T1, T2}.When(Func{int, bool})" />
	IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when setter callback for a set-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
public interface IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, out T1, out T2>
	: IIndexerSetterOnlySetup<TValue, T1, T2>
{
	/// <inheritdoc cref="IIndexerSetterSetupCallbackWhenBuilder{TValue, T1, T2}.For(int)" />
	IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, T1, T2> For(int times);

	/// <inheritdoc cref="IIndexerSetterSetupCallbackWhenBuilder{TValue, T1, T2}.Only(int)" />
	IIndexerSetterOnlySetup<TValue, T1, T2> Only(int times);
}

/// <summary>
///     Setup for a mocked <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" /> that the mock only reads.
/// </summary>
/// <remarks>
///     Used instead of <see cref="IIndexerSetup{TValue, T1, T2, T3}" /> when the mock has no setter to intercept,
///     either because the indexer is declared without one or because its setter is not accessible from the mock's
///     assembly. Writes then never reach the mock, so <see cref="IIndexerSetup{TValue, T1, T2, T3}.OnSet" /> is not
///     offered.
/// </remarks>
public interface IIndexerGetterOnlySetup<TValue, out T1, out T2, out T3>
{
	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.OnGet" />
	IIndexerGetterOnlyGetterSetup<TValue, T1, T2, T3> OnGet { get; }

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.SkippingBaseClass(bool)" />
	IIndexerGetterOnlySetup<TValue, T1, T2, T3> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.InitializeWith(TValue)" />
	/// <remarks>
	///     Seeds the value that reads return. Unlike a read-write indexer there is no setter to update the
	///     slot afterwards, so it stays at <paramref name="value" /> unless a <c>Returns</c> entry applies.
	/// </remarks>
	IIndexerGetterOnlySetup<TValue, T1, T2, T3> InitializeWith(TValue value);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2, T3}.InitializeWith(Func{T1, T2, T3, TValue})" />
	IIndexerGetterOnlySetup<TValue, T1, T2, T3> InitializeWith(Func<T1, T2, T3, TValue> valueGenerator);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Returns(TValue)" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3> Returns(TValue returnValue);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Returns(Func{TValue})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<TValue> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2, T3}.Returns(Func{T1, T2, T3, TValue})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<T1, T2, T3, TValue> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2, T3}.Returns(Func{T1, T2, T3, TValue, TValue})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<T1, T2, T3, TValue, TValue> callback);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Throws{TException}()" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Throws(Exception)" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3> Throws(Exception exception);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Throws(Func{Exception})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3> Throws(Func<Exception> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2, T3}.Throws(Func{T1, T2, T3, Exception})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3> Throws(Func<T1, T2, T3, Exception> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2, T3}.Throws(Func{T1, T2, T3, TValue, Exception})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3> Throws(Func<T1, T2, T3, TValue, Exception> callback);
}

/// <summary>
///     Setup for a mocked <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" /> that the mock only writes.
/// </summary>
/// <remarks>
///     The write-only counterpart of <see cref="IIndexerGetterOnlySetup{TValue, T1, T2, T3}" />: the mock has no
///     getter to intercept, so <see cref="IIndexerSetup{TValue, T1, T2, T3}.OnGet" />, <c>InitializeWith</c> and the
///     <c>Returns</c>/<c>Throws</c> read-sequence are not offered.
/// </remarks>
public interface IIndexerSetterOnlySetup<TValue, out T1, out T2, out T3>
{
	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.OnSet" />
	IIndexerSetterOnlySetterSetup<TValue, T1, T2, T3> OnSet { get; }

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.SkippingBaseClass(bool)" />
	IIndexerSetterOnlySetup<TValue, T1, T2, T3> SkippingBaseClass(bool skipBaseClass = true);
}

/// <summary>
///     Setup for attaching side-effects to the getter of a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
/// <remarks>
///     The counterpart of <see cref="IIndexerGetterSetupWithCallback{TValue, T1, T2, T3}" /> for
///     <see cref="IIndexerGetterOnlySetup{TValue, T1, T2, T3}" />: the returned builders stay on the getter-only
///     surface, so chaining can never reach <see cref="IIndexerSetup{TValue, T1, T2, T3}.OnSet" />.
/// </remarks>
public interface IIndexerGetterOnlyGetterSetup<TValue, out T1, out T2, out T3>
{
	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2, T3}.Do(Action)" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1, T2, T3> Do(Action callback);

	/// <inheritdoc cref="IIndexerGetterSetupWithCallback{TValue, T1, T2, T3}.Do(Action{T1, T2, T3})" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<T1, T2, T3> callback);

	/// <inheritdoc cref="IIndexerGetterSetupWithCallback{TValue, T1, T2, T3}.Do(Action{T1, T2, T3, TValue})" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<T1, T2, T3, TValue> callback);

	/// <inheritdoc cref="IIndexerGetterSetupWithCallback{TValue, T1, T2, T3}.Do(Action{int, T1, T2, T3, TValue})" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<int, T1, T2, T3, TValue> callback);

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2, T3}.TransitionTo(string)" />
	IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, T1, T2, T3> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a callback for a get-only <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerGetterOnlySetupCallbackBuilder<TValue, out T1, out T2, out T3>
	: IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, T1, T2, T3>
{
	/// <inheritdoc cref="IIndexerGetterSetupCallbackBuilder{TValue, T1, T2, T3}.InParallel()" />
	IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, T1, T2, T3> InParallel();
}

/// <summary>
///     Sets up a parallel callback for a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, out T1, out T2, out T3>
	: IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, T1, T2, T3>
{
	/// <inheritdoc cref="IIndexerGetterSetupParallelCallbackBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" />
	IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a get-only <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, out T1, out T2, out T3>
	: IIndexerGetterOnlySetup<TValue, T1, T2, T3>
{
	/// <inheritdoc cref="IIndexerGetterSetupCallbackWhenBuilder{TValue, T1, T2, T3}.For(int)" />
	IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, T1, T2, T3> For(int times);

	/// <inheritdoc cref="IIndexerGetterSetupCallbackWhenBuilder{TValue, T1, T2, T3}.Only(int)" />
	IIndexerGetterOnlySetup<TValue, T1, T2, T3> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerGetterOnlySetupReturnBuilder<TValue, out T1, out T2, out T3>
	: IIndexerGetterOnlySetupReturnWhenBuilder<TValue, T1, T2, T3>
{
	/// <inheritdoc cref="IIndexerSetupReturnBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" />
	IIndexerGetterOnlySetupReturnWhenBuilder<TValue, T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerGetterOnlySetupReturnWhenBuilder<TValue, out T1, out T2, out T3>
	: IIndexerGetterOnlySetup<TValue, T1, T2, T3>
{
	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue, T1, T2, T3}.For(int)" />
	IIndexerGetterOnlySetupReturnWhenBuilder<TValue, T1, T2, T3> For(int times);

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue, T1, T2, T3}.Only(int)" />
	IIndexerGetterOnlySetup<TValue, T1, T2, T3> Only(int times);
}

/// <summary>
///     Setup for attaching side-effects to the setter of a set-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
/// <remarks>
///     The counterpart of <see cref="IIndexerSetterSetupWithCallback{TValue, T1, T2, T3}" /> for
///     <see cref="IIndexerSetterOnlySetup{TValue, T1, T2, T3}" />: the returned builders stay on the setter-only
///     surface, so chaining can never reach <see cref="IIndexerSetup{TValue, T1, T2, T3}.OnGet" /> or the
///     <c>Returns</c>/<c>Throws</c> read-sequence.
/// </remarks>
public interface IIndexerSetterOnlySetterSetup<TValue, out T1, out T2, out T3>
{
	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3}.Do(Action)" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1, T2, T3> Do(Action callback);

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3}.Do(Action{TValue})" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<TValue> callback);

	/// <inheritdoc cref="IIndexerSetterSetupWithCallback{TValue, T1, T2, T3}.Do(Action{T1, T2, T3, TValue})" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<T1, T2, T3, TValue> callback);

	/// <inheritdoc cref="IIndexerSetterSetupWithCallback{TValue, T1, T2, T3}.Do(Action{int, T1, T2, T3, TValue})" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1, T2, T3> Do(Action<int, T1, T2, T3, TValue> callback);

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3}.TransitionTo(string)" />
	IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, T1, T2, T3> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a setter callback for a set-only <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerSetterOnlySetupCallbackBuilder<TValue, out T1, out T2, out T3>
	: IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, T1, T2, T3>
{
	/// <inheritdoc cref="IIndexerSetterSetupCallbackBuilder{TValue, T1, T2, T3}.InParallel()" />
	IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, T1, T2, T3> InParallel();
}

/// <summary>
///     Sets up a parallel setter callback for a set-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, out T1, out T2, out T3>
	: IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, T1, T2, T3>
{
	/// <inheritdoc cref="IIndexerSetterSetupParallelCallbackBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" />
	IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when setter callback for a set-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" /> and <typeparamref name="T3" />.
/// </summary>
public interface IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, out T1, out T2, out T3>
	: IIndexerSetterOnlySetup<TValue, T1, T2, T3>
{
	/// <inheritdoc cref="IIndexerSetterSetupCallbackWhenBuilder{TValue, T1, T2, T3}.For(int)" />
	IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, T1, T2, T3> For(int times);

	/// <inheritdoc cref="IIndexerSetterSetupCallbackWhenBuilder{TValue, T1, T2, T3}.Only(int)" />
	IIndexerSetterOnlySetup<TValue, T1, T2, T3> Only(int times);
}

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
/// <summary>
///     Setup for a mocked <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" /> that the mock only
///     reads.
/// </summary>
/// <remarks>
///     Used instead of <see cref="IIndexerSetup{TValue, T1, T2, T3, T4}" /> when the mock has no setter to intercept,
///     either because the indexer is declared without one or because its setter is not accessible from the mock's
///     assembly. Writes then never reach the mock, so <see cref="IIndexerSetup{TValue, T1, T2, T3, T4}.OnSet" /> is
///     not offered.
/// </remarks>
public interface IIndexerGetterOnlySetup<TValue, out T1, out T2, out T3, out T4>
{
	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.OnGet" />
	IIndexerGetterOnlyGetterSetup<TValue, T1, T2, T3, T4> OnGet { get; }

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.SkippingBaseClass(bool)" />
	IIndexerGetterOnlySetup<TValue, T1, T2, T3, T4> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.InitializeWith(TValue)" />
	/// <remarks>
	///     Seeds the value that reads return. Unlike a read-write indexer there is no setter to update the
	///     slot afterwards, so it stays at <paramref name="value" /> unless a <c>Returns</c> entry applies.
	/// </remarks>
	IIndexerGetterOnlySetup<TValue, T1, T2, T3, T4> InitializeWith(TValue value);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2, T3, T4}.InitializeWith(Func{T1, T2, T3, T4, TValue})" />
	IIndexerGetterOnlySetup<TValue, T1, T2, T3, T4> InitializeWith(Func<T1, T2, T3, T4, TValue> valueGenerator);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Returns(TValue)" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(TValue returnValue);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Returns(Func{TValue})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<TValue> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2, T3, T4}.Returns(Func{T1, T2, T3, T4, TValue})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<T1, T2, T3, T4, TValue> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2, T3, T4}.Returns(Func{T1, T2, T3, T4, TValue, TValue})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<T1, T2, T3, T4, TValue, TValue> callback);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Throws{TException}()" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Throws(Exception)" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Exception exception);

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Throws(Func{Exception})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Func<Exception> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2, T3, T4}.Throws(Func{T1, T2, T3, T4, Exception})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, Exception> callback);

	/// <inheritdoc cref="IIndexerSetupWithCallback{TValue, T1, T2, T3, T4}.Throws(Func{T1, T2, T3, T4, TValue, Exception})" />
	IIndexerGetterOnlySetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, TValue, Exception> callback);
}

/// <summary>
///     Setup for a mocked <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" /> that the mock only
///     writes.
/// </summary>
/// <remarks>
///     The write-only counterpart of <see cref="IIndexerGetterOnlySetup{TValue, T1, T2, T3, T4}" />: the mock has no
///     getter to intercept, so <see cref="IIndexerSetup{TValue, T1, T2, T3, T4}.OnGet" />, <c>InitializeWith</c> and
///     the <c>Returns</c>/<c>Throws</c> read-sequence are not offered.
/// </remarks>
public interface IIndexerSetterOnlySetup<TValue, out T1, out T2, out T3, out T4>
{
	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.OnSet" />
	IIndexerSetterOnlySetterSetup<TValue, T1, T2, T3, T4> OnSet { get; }

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.SkippingBaseClass(bool)" />
	IIndexerSetterOnlySetup<TValue, T1, T2, T3, T4> SkippingBaseClass(bool skipBaseClass = true);
}

/// <summary>
///     Setup for attaching side-effects to the getter of a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" />, <typeparamref name="T3" /> and
///     <typeparamref name="T4" />.
/// </summary>
/// <remarks>
///     The counterpart of <see cref="IIndexerGetterSetupWithCallback{TValue, T1, T2, T3, T4}" /> for
///     <see cref="IIndexerGetterOnlySetup{TValue, T1, T2, T3, T4}" />: the returned builders stay on the getter-only
///     surface, so chaining can never reach <see cref="IIndexerSetup{TValue, T1, T2, T3, T4}.OnSet" />.
/// </remarks>
public interface IIndexerGetterOnlyGetterSetup<TValue, out T1, out T2, out T3, out T4>
{
	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2, T3, T4}.Do(Action)" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action callback);

	/// <inheritdoc cref="IIndexerGetterSetupWithCallback{TValue, T1, T2, T3, T4}.Do(Action{T1, T2, T3, T4})" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<T1, T2, T3, T4> callback);

	/// <inheritdoc cref="IIndexerGetterSetupWithCallback{TValue, T1, T2, T3, T4}.Do(Action{T1, T2, T3, T4, TValue})" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<T1, T2, T3, T4, TValue> callback);

	/// <inheritdoc cref="IIndexerGetterSetupWithCallback{TValue, T1, T2, T3, T4}.Do(Action{int, T1, T2, T3, T4, TValue})" />
	IIndexerGetterOnlySetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<int, T1, T2, T3, T4, TValue> callback);

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2, T3, T4}.TransitionTo(string)" />
	IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, T1, T2, T3, T4> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a callback for a get-only <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface IIndexerGetterOnlySetupCallbackBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, T1, T2, T3, T4>
{
	/// <inheritdoc cref="IIndexerGetterSetupCallbackBuilder{TValue, T1, T2, T3, T4}.InParallel()" />
	IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, T1, T2, T3, T4> InParallel();
}

/// <summary>
///     Sets up a parallel callback for a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" />, <typeparamref name="T3" /> and
///     <typeparamref name="T4" />.
/// </summary>
public interface IIndexerGetterOnlySetupParallelCallbackBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, T1, T2, T3, T4>
{
	/// <inheritdoc cref="IIndexerGetterSetupParallelCallbackBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" />
	IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a get-only <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerGetterOnlySetup<TValue, T1, T2, T3, T4>
{
	/// <inheritdoc cref="IIndexerGetterSetupCallbackWhenBuilder{TValue, T1, T2, T3, T4}.For(int)" />
	IIndexerGetterOnlySetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> For(int times);

	/// <inheritdoc cref="IIndexerGetterSetupCallbackWhenBuilder{TValue, T1, T2, T3, T4}.Only(int)" />
	IIndexerGetterOnlySetup<TValue, T1, T2, T3, T4> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" />, <typeparamref name="T3" /> and
///     <typeparamref name="T4" />.
/// </summary>
public interface IIndexerGetterOnlySetupReturnBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerGetterOnlySetupReturnWhenBuilder<TValue, T1, T2, T3, T4>
{
	/// <inheritdoc cref="IIndexerSetupReturnBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" />
	IIndexerGetterOnlySetupReturnWhenBuilder<TValue, T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a get-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" />, <typeparamref name="T3" /> and
///     <typeparamref name="T4" />.
/// </summary>
public interface IIndexerGetterOnlySetupReturnWhenBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerGetterOnlySetup<TValue, T1, T2, T3, T4>
{
	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue, T1, T2, T3, T4}.For(int)" />
	IIndexerGetterOnlySetupReturnWhenBuilder<TValue, T1, T2, T3, T4> For(int times);

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue, T1, T2, T3, T4}.Only(int)" />
	IIndexerGetterOnlySetup<TValue, T1, T2, T3, T4> Only(int times);
}

/// <summary>
///     Setup for attaching side-effects to the setter of a set-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" />, <typeparamref name="T3" /> and
///     <typeparamref name="T4" />.
/// </summary>
/// <remarks>
///     The counterpart of <see cref="IIndexerSetterSetupWithCallback{TValue, T1, T2, T3, T4}" /> for
///     <see cref="IIndexerSetterOnlySetup{TValue, T1, T2, T3, T4}" />: the returned builders stay on the setter-only
///     surface, so chaining can never reach <see cref="IIndexerSetup{TValue, T1, T2, T3, T4}.OnGet" /> or the
///     <c>Returns</c>/<c>Throws</c> read-sequence.
/// </remarks>
public interface IIndexerSetterOnlySetterSetup<TValue, out T1, out T2, out T3, out T4>
{
	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3, T4}.Do(Action)" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action callback);

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3, T4}.Do(Action{TValue})" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<TValue> callback);

	/// <inheritdoc cref="IIndexerSetterSetupWithCallback{TValue, T1, T2, T3, T4}.Do(Action{T1, T2, T3, T4, TValue})" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<T1, T2, T3, T4, TValue> callback);

	/// <inheritdoc cref="IIndexerSetterSetupWithCallback{TValue, T1, T2, T3, T4}.Do(Action{int, T1, T2, T3, T4, TValue})" />
	IIndexerSetterOnlySetupCallbackBuilder<TValue, T1, T2, T3, T4> Do(Action<int, T1, T2, T3, T4, TValue> callback);

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3, T4}.TransitionTo(string)" />
	IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, T1, T2, T3, T4> TransitionTo(string scenario);
}

/// <summary>
///     Sets up a setter callback for a set-only <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />,
///     <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public interface IIndexerSetterOnlySetupCallbackBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, T1, T2, T3, T4>
{
	/// <inheritdoc cref="IIndexerSetterSetupCallbackBuilder{TValue, T1, T2, T3, T4}.InParallel()" />
	IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, T1, T2, T3, T4> InParallel();
}

/// <summary>
///     Sets up a parallel setter callback for a set-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" />, <typeparamref name="T3" /> and
///     <typeparamref name="T4" />.
/// </summary>
public interface IIndexerSetterOnlySetupParallelCallbackBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, T1, T2, T3, T4>
{
	/// <inheritdoc cref="IIndexerSetterSetupParallelCallbackBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" />
	IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when setter callback for a set-only <typeparamref name="TValue" /> indexer for
///     <typeparamref name="T1" />, <typeparamref name="T2" />, <typeparamref name="T3" /> and
///     <typeparamref name="T4" />.
/// </summary>
public interface IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, out T1, out T2, out T3, out T4>
	: IIndexerSetterOnlySetup<TValue, T1, T2, T3, T4>
{
	/// <inheritdoc cref="IIndexerSetterSetupCallbackWhenBuilder{TValue, T1, T2, T3, T4}.For(int)" />
	IIndexerSetterOnlySetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> For(int times);

	/// <inheritdoc cref="IIndexerSetterSetupCallbackWhenBuilder{TValue, T1, T2, T3, T4}.Only(int)" />
	IIndexerSetterOnlySetup<TValue, T1, T2, T3, T4> Only(int times);
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
