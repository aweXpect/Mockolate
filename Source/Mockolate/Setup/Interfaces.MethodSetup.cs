using System;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Interface for hiding some implementation details of <see cref="MethodSetup" />.
/// </summary>
public interface IMethodSetup
{
	/// <summary>
	///     Checks if the <paramref name="methodInvocation" /> matches the setup.
	/// </summary>
	bool Matches(MethodInvocation methodInvocation);

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be called, and its return values
	///     used as default values.
	/// </summary>
	bool? CallBaseClass();

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
	///     is generated using the current <paramref name="behavior" />.
	/// </remarks>
	T SetOutParameter<T>(string parameterName, MockBehavior behavior);

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
	TResult Invoke<TResult>(MethodInvocation methodInvocation, MockBehavior behavior);

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
public interface IReturnMethodSetup<in TReturn>
{
	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	IReturnMethodSetup<TReturn> CallingBaseClass(bool callBaseClass = true);

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
public interface IReturnMethodSetupCallbackBuilder<in TReturn> : IReturnMethodSetupCallbackWhenBuilder<TReturn>
{
	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	IReturnMethodSetupCallbackWhenBuilder<TReturn> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupCallbackWhenBuilder<in TReturn> : IReturnMethodSetup<TReturn>
{
	/// <summary>
	///     Limits the callback to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn> For(int times);
}

/// <summary>
///     Sets up a return/throw builder for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnBuilder<in TReturn> : IReturnMethodSetupReturnWhenBuilder<TReturn>
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
public interface IReturnMethodSetupReturnWhenBuilder<in TReturn> : IReturnMethodSetup<TReturn>
{
	/// <summary>
	///     Limits the return/throw to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn> For(int times);
}

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetup<in TReturn, out T1>
{
	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	IReturnMethodSetup<TReturn, T1> CallingBaseClass(bool callBaseClass = true);

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
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
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
	///     Limits the callback to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1> For(int times);
}

/// <summary>
///     Sets up a return/throw builder for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnBuilder<in TReturn, out T1> : IReturnMethodSetupReturnWhenBuilder<TReturn, T1>
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
public interface IReturnMethodSetupReturnWhenBuilder<in TReturn, out T1> : IReturnMethodSetup<TReturn, T1>
{
	/// <summary>
	///     Limits the return/throw to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn, T1}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1> For(int times);
}

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetup<in TReturn, out T1, out T2>
{
	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2> CallingBaseClass(bool callBaseClass = true);

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
	: IReturnMethodSetupCallbackWhenBuilder<TReturn, T1,
		T2>
{
	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
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
	///     Limits the callback to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2> For(int times);
}

/// <summary>
///     Sets up a return/throw builder for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnBuilder<in TReturn, out T1, out T2> : IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2>
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
public interface IReturnMethodSetupReturnWhenBuilder<in TReturn, out T1, out T2> : IReturnMethodSetup<TReturn, T1, T2>
{
	/// <summary>
	///     Limits the return/throw to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2> For(int times);
}

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetup<in TReturn, out T1, out T2, out T3>
{
	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2, T3> CallingBaseClass(bool callBaseClass = true);

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
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
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
	///     Limits the callback to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2, T3> For(int times);
}

/// <summary>
///     Sets up a return/throw builder for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnBuilder<in TReturn, out T1, out T2, out T3> : IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3>
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
public interface IReturnMethodSetupReturnWhenBuilder<in TReturn, out T1, out T2, out T3> : IReturnMethodSetup<TReturn, T1, T2, T3>
{
	/// <summary>
	///     Limits the return/throw to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2, T3> For(int times);
}

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetup<in TReturn, out T1, out T2, out T3, out T4>
{
	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2, T3, T4> CallingBaseClass(bool callBaseClass = true);

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
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
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
	///     Limits the callback to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2, T3, T4> For(int times);
}

/// <summary>
///     Sets up a return/throw builder for a method returning <typeparamref name="TReturn" />.
/// </summary>
public interface IReturnMethodSetupReturnBuilder<in TReturn, out T1, out T2, out T3, out T4> : IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3, T4>
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
public interface IReturnMethodSetupReturnWhenBuilder<in TReturn, out T1, out T2, out T3, out T4> : IReturnMethodSetup<TReturn, T1, T2, T3, T4>
{
	/// <summary>
	///     Limits the return/throw to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />
	///     ).
	/// </remarks>
	IReturnMethodSetup<TReturn, T1, T2, T3, T4> For(int times);
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters

/// <summary>
///     Sets up a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetup
{
	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	IVoidMethodSetup CallingBaseClass(bool callBaseClass = true);

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
	IVoidMethodSetupThrowBuilder Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder Throws(Func<Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackBuilder : IVoidMethodSetupCallbackWhenBuilder
{
	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackWhenBuilder : IVoidMethodSetup
{
	/// <summary>
	///     Limits the callback to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup For(int times);
}

/// <summary>
///     Sets up a throw builder for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupThrowBuilder : IVoidMethodSetupThrowWhenBuilder
{
	/// <summary>
	///     Limits the throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupThrowWhenBuilder When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for throws for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupThrowWhenBuilder : IVoidMethodSetup
{
	/// <summary>
	///     Limits the throw to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupThrowBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup For(int times);
}

/// <summary>
///     Sets up a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetup<out T1>
{
	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	IVoidMethodSetup<T1> CallingBaseClass(bool callBaseClass = true);

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
	IVoidMethodSetupThrowBuilder<T1> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder<T1> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder<T1> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder<T1> Throws(Func<T1, Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackBuilder<out T1> : IVoidMethodSetupCallbackWhenBuilder<T1>
{
	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder<T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackWhenBuilder<out T1> : IVoidMethodSetup<T1>
{
	/// <summary>
	///     Limits the callback to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1> For(int times);
}

/// <summary>
///     Sets up a throw builder for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupThrowBuilder<out T1> : IVoidMethodSetupThrowWhenBuilder<T1>
{
	/// <summary>
	///     Limits the throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupThrowWhenBuilder<T1> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for throws for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupThrowWhenBuilder<out T1> : IVoidMethodSetup<T1>
{
	/// <summary>
	///     Limits the throw to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupThrowBuilder{T1}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1> For(int times);
}

/// <summary>
///     Sets up a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetup<out T1, out T2>
{
	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	IVoidMethodSetup<T1, T2> CallingBaseClass(bool callBaseClass = true);

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
	IVoidMethodSetupThrowBuilder<T1, T2> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder<T1, T2> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder<T1, T2> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder<T1, T2> Throws(Func<T1, T2, Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackBuilder<out T1, out T2> : IVoidMethodSetupCallbackWhenBuilder<T1, T2>
{
	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder<T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackWhenBuilder<out T1, out T2> : IVoidMethodSetup<T1, T2>
{
	/// <summary>
	///     Limits the callback to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1, T2> For(int times);
}

/// <summary>
///     Sets up a throw builder for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupThrowBuilder<out T1, out T2> : IVoidMethodSetupThrowWhenBuilder<T1, T2>
{
	/// <summary>
	///     Limits the throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupThrowWhenBuilder<T1, T2> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for throws for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupThrowWhenBuilder<out T1, out T2> : IVoidMethodSetup<T1, T2>
{
	/// <summary>
	///     Limits the throw to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupThrowBuilder{T1, T2}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1, T2> For(int times);
}

/// <summary>
///     Sets up a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetup<out T1, out T2, out T3>
{
	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	IVoidMethodSetup<T1, T2, T3> CallingBaseClass(bool callBaseClass = true);

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
	IVoidMethodSetupThrowBuilder<T1, T2, T3> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder<T1, T2, T3> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder<T1, T2, T3> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder<T1, T2, T3> Throws(Func<T1, T2, T3, Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <see langword="void" />.
/// </summary>
public interface
	IVoidMethodSetupCallbackBuilder<out T1, out T2, out T3> : IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3>
{
	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackWhenBuilder<out T1, out T2, out T3> : IVoidMethodSetup<T1, T2, T3>
{
	/// <summary>
	///     Limits the callback to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1, T2, T3> For(int times);
}

/// <summary>
///     Sets up a throw builder for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupThrowBuilder<out T1, out T2, out T3> : IVoidMethodSetupThrowWhenBuilder<T1, T2, T3>
{
	/// <summary>
	///     Limits the throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupThrowWhenBuilder<T1, T2, T3> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for throws for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupThrowWhenBuilder<out T1, out T2, out T3> : IVoidMethodSetup<T1, T2, T3>
{
	/// <summary>
	///     Limits the throw to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupThrowBuilder{T1, T2, T3}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1, T2, T3> For(int times);
}

/// <summary>
///     Sets up a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetup<out T1, out T2, out T3, out T4>
{
	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	IVoidMethodSetup<T1, T2, T3, T4> CallingBaseClass(bool callBaseClass = true);

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
	IVoidMethodSetupThrowBuilder<T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder<T1, T2, T3, T4> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder<T1, T2, T3, T4> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	IVoidMethodSetupThrowBuilder<T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, Exception> callback);
}

/// <summary>
///     Sets up a callback for a method returning <see langword="void" />.
/// </summary>
public interface
	IVoidMethodSetupCallbackBuilder<out T1, out T2, out T3, out T4> : IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3,
	T4>
{
	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when callback for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupCallbackWhenBuilder<out T1, out T2, out T3, out T4> : IVoidMethodSetup<T1, T2, T3, T4>
{
	/// <summary>
	///     Limits the callback to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1, T2, T3, T4> For(int times);
}

/// <summary>
///     Sets up a throw builder for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupThrowBuilder<out T1, out T2, out T3, out T4> : IVoidMethodSetupThrowWhenBuilder<T1, T2, T3, T4>
{
	/// <summary>
	///     Limits the throw to only execute for method invocations where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the method has been invoked so far.
	/// </remarks>
	IVoidMethodSetupThrowWhenBuilder<T1, T2, T3, T4> When(Func<int, bool> predicate);
}

/// <summary>
///     Sets up a when builder for throws for a method returning <see langword="void" />.
/// </summary>
public interface IVoidMethodSetupThrowWhenBuilder<out T1, out T2, out T3, out T4> : IVoidMethodSetup<T1, T2, T3, T4>
{
	/// <summary>
	///     Limits the throw to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IVoidMethodSetupThrowBuilder{T1, T2, T3, T4}.When(Func{int, bool})" /> evaluates to
	///     <see langword="true" />).
	/// </remarks>
	IVoidMethodSetup<T1, T2, T3, T4> For(int times);
}
