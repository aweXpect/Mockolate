using System;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Marker interface for method setups.
/// </summary>
public interface IMethodSetup;

/// <summary>
///     Interface for verifiable method setup. It hides the implementation details to get the underlying
///     <see cref="IMethodMatch" />.
/// </summary>
public interface IVerifiableMethodSetup
{
	/// <summary>
	///     Gets the <see cref="IMethodMatch" /> used to match against method invocations.
	/// </summary>
	IMethodMatch GetMatch();
}

/// <summary>
///     Interface for hiding some implementation details of <see cref="MethodSetup" />.
/// </summary>
public interface IInteractiveMethodSetup : ISetup
{
	/// <summary>
	///     Checks if the <paramref name="methodInvocation" /> matches the setup.
	/// </summary>
	bool Matches(MethodInvocation methodInvocation);

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	bool? SkipBaseClass();

	/// <summary>
	///     Gets a value indicating whether this setup has return calls configured.
	/// </summary>
	bool HasReturnCalls();

	/// <summary>
	///     Sets an <see langword="out" /> parameter with the specified name and returns its generated value of type
	///     <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     If a setup is configured, the value is generated according to the setup; otherwise, a default value
	///     is generated using the <paramref name="defaultValueGenerator" />.
	/// </remarks>
	T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator);

	/// <summary>
	///     Sets an <see langword="ref" /> parameter with the specified name and the initial <paramref name="value" /> and
	///     returns its generated value of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     If a setup is configured, the value is generated according to the setup; otherwise, a default value
	///     is generated using the current <paramref name="behavior" />.
	/// </remarks>
	T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior);

	/// <summary>
	///     Invokes the <paramref name="methodInvocation" /> returning a value of type <typeparamref name="TResult" />.
	/// </summary>
	TResult Invoke<TResult>(MethodInvocation methodInvocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator);

	/// <summary>
	///     Invokes the <paramref name="methodInvocation" /> returning <see langword="void" />.
	/// </summary>
	void Invoke(MethodInvocation methodInvocation, MockBehavior behavior);

	/// <summary>
	///     Triggers any configured parameter callbacks for the method setup with the specified <paramref name="parameters" />.
	/// </summary>
	void TriggerCallbacks(object?[] parameters);
}

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetup<in TReturn> : IMethodSetup
{
	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IReturnMethodSetup<TReturn> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn> Do(Action callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn> Do(Action<int> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn> Returns(Func<TReturn> callback);

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn> Returns(TReturn returnValue);

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn> Throws(Func<Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupCallbackBuilder<in TReturn>
	: IReturnMethodSetupCallbackWhenBuilder<TReturn>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn> InParallel();

	/// <summary>
	///     Limits the callback to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IReturnMethodSetupCallbackWhenBuilder<TReturn> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupCallbackWhenBuilder<in TReturn>
	: IReturnMethodSetup<TReturn>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetupCallbackWhenBuilder<TReturn> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnBuilder<in TReturn>
	: IReturnMethodSetupReturnWhenBuilder<TReturn>
{
	/// <summary>
	///     Limits the return/throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IReturnMethodSetupReturnWhenBuilder<TReturn> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnWhenBuilder<in TReturn>
	: IReturnMethodSetup<TReturn>
{
	/// <summary>
	///     Repeats the return/throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetupReturnWhenBuilder<TReturn> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn> Only(int times);
}

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetup<in TReturn, out T1> : IMethodSetup
{
	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IReturnMethodSetup<TReturn, T1> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1> Do(Action callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1> Do(Action<T1> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1> Do(Action<int, T1> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1> Returns(Func<T1, TReturn> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1> Returns(Func<TReturn> callback);

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1> Returns(TReturn returnValue);

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1> Throws(Func<T1, Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupCallbackBuilder<in TReturn, out T1>
	: IReturnMethodSetupCallbackWhenBuilder<TReturn, T1>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1> InParallel();

	/// <summary>
	///     Limits the callback to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupCallbackWhenBuilder<in TReturn, out T1>
	: IReturnMethodSetup<TReturn, T1>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnBuilder<in TReturn, out T1>
	: IReturnMethodSetupReturnWhenBuilder<TReturn, T1>
{
	/// <summary>
	///     Limits the return/throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnWhenBuilder<in TReturn, out T1>
	: IReturnMethodSetup<TReturn, T1>
{
	/// <summary>
	///     Repeats the return/throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn, T1}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1> For(int times);

	/// <summary>
	///     Deactivates the return/throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn, T1}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1> Only(int times);
}

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetup<in TReturn, out T1, out T2> : IMethodSetup
{
	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2> Do(Action callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2> Do(Action<T1, T2> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2> Do(Action<int, T1, T2> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Returns(Func<T1, T2, TReturn> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Returns(Func<TReturn> callback);

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Returns(TReturn returnValue);

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Throws(Func<T1, T2, Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupCallbackBuilder<in TReturn, out T1, out T2>
	: IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2> InParallel();

	/// <summary>
	///     Limits the callback to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupCallbackWhenBuilder<in TReturn, out T1, out T2>
	: IReturnMethodSetup<TReturn, T1, T2>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnBuilder<in TReturn, out T1, out T2>
	: IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2>
{
	/// <summary>
	///     Limits the return/throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnWhenBuilder<in TReturn, out T1, out T2>
	: IReturnMethodSetup<TReturn, T1, T2>
{
	/// <summary>
	///     Repeats the return/throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2> For(int times);

	/// <summary>
	///     Deactivates the return/throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2> Only(int times);
}

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetup<in TReturn, out T1, out T2, out T3> : IMethodSetup
{
	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2, T3> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3> Do(Action callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3> Do(Action<T1, T2, T3> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3> Do(Action<int, T1, T2, T3> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Returns(Func<T1, T2, T3, TReturn> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Returns(Func<TReturn> callback);

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Returns(TReturn returnValue);

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Throws(Func<T1, T2, T3, Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupCallbackBuilder<in TReturn, out T1, out T2, out T3>
	: IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3> InParallel();

	/// <summary>
	///     Limits the callback to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupCallbackWhenBuilder<in TReturn, out T1, out T2, out T3>
	: IReturnMethodSetup<TReturn, T1, T2, T3>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2, T3> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnBuilder<in TReturn, out T1, out T2, out T3>
	: IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3>
{
	/// <summary>
	///     Limits the return/throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnWhenBuilder<in TReturn, out T1, out T2, out T3>
	: IReturnMethodSetup<TReturn, T1, T2, T3>
{
	/// <summary>
	///     Repeats the return/throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3> For(int times);

	/// <summary>
	///     Deactivates the return/throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2, T3> Only(int times);
}

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetup<in TReturn, out T1, out T2, out T3, out T4> : IMethodSetup
{
	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2, T3, T4> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4> Do(Action callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4> Do(Action<T1, T2, T3, T4> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4> Do(Action<int, T1, T2, T3, T4> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Returns(Func<T1, T2, T3, T4, TReturn> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Returns(Func<TReturn> callback);

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Returns(TReturn returnValue);

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupCallbackBuilder<in TReturn, out T1, out T2, out T3, out T4>
	: IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3, T4>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4> InParallel();

	/// <summary>
	///     Limits the callback to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupCallbackWhenBuilder<in TReturn, out T1, out T2, out T3, out T4>
	: IReturnMethodSetup<TReturn, T1, T2, T3, T4>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3, T4> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2, T3, T4> Only(int times);
}

/// <summary>
///     Sets up a return/throw builder for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnBuilder<in TReturn, out T1, out T2, out T3, out T4>
	: IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3, T4>
{
	/// <summary>
	///     Limits the return/throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for returns/throws for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnWhenBuilder<in TReturn, out T1, out T2, out T3, out T4>
	: IReturnMethodSetup<TReturn, T1, T2, T3, T4>
{
	/// <summary>
	///     Repeats the return/throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3, T4> For(int times);

	/// <summary>
	///     Deactivates the return/throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2, T3, T4> Only(int times);
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters

/// <summary>
///     Sets up a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetup : IMethodSetup
{
	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IVoidMethodSetup SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder Do(Action callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder Do(Action<int> callback);

	/// <summary>
	///     Registers an iteration in the sequence of method invocations, that does not throw.
	/// </summary>
	IVoidMethodSetup DoesNotThrow();

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder Throws(Func<Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackBuilder : IVoidMethodSetupCallbackWhenBuilder
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IVoidMethodSetupCallbackBuilder InParallel();

	/// <summary>
	///     Limits the callback to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackWhenBuilder : IVoidMethodSetup
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup Only(int times);
}

/// <summary>
///     Sets up a throw builder for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupReturnBuilder : IVoidMethodSetupReturnWhenBuilder
{
	/// <summary>
	///     Limits the throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupReturnWhenBuilder When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for throws for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupReturnWhenBuilder : IVoidMethodSetup
{
	/// <summary>
	///     Repeats the throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupReturnBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetupReturnWhenBuilder For(int times);

	/// <summary>
	///     Deactivates the throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupReturnBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup Only(int times);
}

/// <summary>
///     Sets up a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetup<out T1> : IMethodSetup
{
	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IVoidMethodSetup<T1> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1> Do(Action callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1> Do(Action<T1> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1> Do(Action<int, T1> callback);

	/// <summary>
	///     Registers an iteration in the sequence of method invocations, that does not throw.
	/// </summary>
	IVoidMethodSetup<T1> DoesNotThrow();

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1> Throws(Func<T1, Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackBuilder<out T1>
	: IVoidMethodSetupCallbackWhenBuilder<T1>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1> InParallel();

	/// <summary>
	///     Limits the callback to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder<T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackWhenBuilder<out T1>
	: IVoidMethodSetup<T1>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder<T1> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1> Only(int times);
}

/// <summary>
///     Sets up a throw builder for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupReturnBuilder<out T1>
	: IVoidMethodSetupReturnWhenBuilder<T1>
{
	/// <summary>
	///     Limits the throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupReturnWhenBuilder<T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for throws for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupReturnWhenBuilder<out T1>
	: IVoidMethodSetup<T1>
{
	/// <summary>
	///     Repeats the throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupReturnBuilder{T1}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetupReturnWhenBuilder<T1> For(int times);

	/// <summary>
	///     Deactivates the throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupReturnBuilder{T1}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1> Only(int times);
}

/// <summary>
///     Sets up a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetup<out T1, out T2> : IMethodSetup
{
	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IVoidMethodSetup<T1, T2> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1, T2> Do(Action callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1, T2> Do(Action<T1, T2> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1, T2> Do(Action<int, T1, T2> callback);

	/// <summary>
	///     Registers an iteration in the sequence of method invocations, that does not throw.
	/// </summary>
	IVoidMethodSetup<T1, T2> DoesNotThrow();

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1, T2> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1, T2> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1, T2> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1, T2> Throws(Func<T1, T2, Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackBuilder<out T1, out T2>
	: IVoidMethodSetupCallbackWhenBuilder<T1, T2>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1, T2> InParallel();

	/// <summary>
	///     Limits the callback to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder<T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackWhenBuilder<out T1, out T2>
	: IVoidMethodSetup<T1, T2>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder<T1, T2> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1, T2> Only(int times);
}

/// <summary>
///     Sets up a throw builder for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupReturnBuilder<out T1, out T2>
	: IVoidMethodSetupReturnWhenBuilder<T1, T2>
{
	/// <summary>
	///     Limits the throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupReturnWhenBuilder<T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for throws for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupReturnWhenBuilder<out T1, out T2>
	: IVoidMethodSetup<T1, T2>
{
	/// <summary>
	///     Repeats the throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupReturnBuilder{T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetupReturnWhenBuilder<T1, T2> For(int times);

	/// <summary>
	///     Deactivates the throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupReturnBuilder{T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1, T2> Only(int times);
}

/// <summary>
///     Sets up a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetup<out T1, out T2, out T3> : IMethodSetup
{
	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IVoidMethodSetup<T1, T2, T3> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1, T2, T3> Do(Action callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1, T2, T3> Do(Action<T1, T2, T3> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1, T2, T3> Do(Action<int, T1, T2, T3> callback);

	/// <summary>
	///     Registers an iteration in the sequence of method invocations, that does not throw.
	/// </summary>
	IVoidMethodSetup<T1, T2, T3> DoesNotThrow();

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1, T2, T3> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1, T2, T3> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1, T2, T3> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1, T2, T3> Throws(Func<T1, T2, T3, Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackBuilder<out T1, out T2, out T3>
	: IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1, T2, T3> InParallel();

	/// <summary>
	///     Limits the callback to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackWhenBuilder<out T1, out T2, out T3>
	: IVoidMethodSetup<T1, T2, T3>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1, T2, T3> Only(int times);
}

/// <summary>
///     Sets up a throw builder for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupReturnBuilder<out T1, out T2, out T3>
	: IVoidMethodSetupReturnWhenBuilder<T1, T2, T3>
{
	/// <summary>
	///     Limits the throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupReturnWhenBuilder<T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for throws for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupReturnWhenBuilder<out T1, out T2, out T3>
	: IVoidMethodSetup<T1, T2, T3>
{
	/// <summary>
	///     Repeats the throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupReturnBuilder{T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetupReturnWhenBuilder<T1, T2, T3> For(int times);

	/// <summary>
	///     Deactivates the throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupReturnBuilder{T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1, T2, T3> Only(int times);
}

/// <summary>
///     Sets up a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetup<out T1, out T2, out T3, out T4> : IMethodSetup
{
	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IVoidMethodSetup<T1, T2, T3, T4> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1, T2, T3, T4> Do(Action callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1, T2, T3, T4> Do(Action<T1, T2, T3, T4> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1, T2, T3, T4> Do(Action<int, T1, T2, T3, T4> callback);

	/// <summary>
	///     Registers an iteration in the sequence of method invocations, that does not throw.
	/// </summary>
	IVoidMethodSetup<T1, T2, T3, T4> DoesNotThrow();

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1, T2, T3, T4> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1, T2, T3, T4> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupReturnBuilder<T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackBuilder<out T1, out T2, out T3, out T4>
	: IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3, T4>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IVoidMethodSetupCallbackBuilder<T1, T2, T3, T4> InParallel();

	/// <summary>
	///     Limits the callback to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackWhenBuilder<out T1, out T2, out T3, out T4>
	: IVoidMethodSetup<T1, T2, T3, T4>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3, T4> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1, T2, T3, T4> Only(int times);
}

/// <summary>
///     Sets up a throw builder for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupReturnBuilder<out T1, out T2, out T3, out T4>
	: IVoidMethodSetupReturnWhenBuilder<T1, T2, T3, T4>
{
	/// <summary>
	///     Limits the throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupReturnWhenBuilder<T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for throws for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupReturnWhenBuilder<out T1, out T2, out T3, out T4>
	: IVoidMethodSetup<T1, T2, T3, T4>
{
	/// <summary>
	///     Repeats the throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupReturnBuilder{T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetupReturnWhenBuilder<T1, T2, T3, T4> For(int times);

	/// <summary>
	///     Deactivates the throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupReturnBuilder{T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1, T2, T3, T4> Only(int times);
}
