using System;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Interface for hiding some implementation details of <see cref="PropertySetup" />.
/// </summary>
public interface IInteractivePropertySetup : ISetup
{
	/// <summary>
	///     Invokes the setter logic for the <paramref name="invocation" /> and <paramref name="value" />.
	/// </summary>
	void InvokeSetter(IInteraction invocation, object? value, MockBehavior behavior);

	/// <summary>
	///     Invokes the getter logic for the <paramref name="invocation" /> and returns the value of type
	///     <typeparamref name="TResult" />.
	/// </summary>
	TResult InvokeGetter<TResult>(IInteraction invocation, MockBehavior behavior, Func<TResult> defaultValueGenerator);

	/// <summary>
	///     Checks if the <paramref name="propertyAccess" /> matches the setup.
	/// </summary>
	bool Matches(PropertyAccess propertyAccess);

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     When not explicitly set on the <see cref="IPropertySetup{T}" />, returns <see langword="null" />.
	/// </remarks>
	bool? SkipBaseClass();

	/// <summary>
	///     Initialize the <see cref="IPropertySetup{T}" /> with the <paramref name="value" />.
	/// </summary>
	void InitializeWith(object? value);
}

/// <summary>
///     Interface for setting up a property getter with fluent syntax.
/// </summary>
public interface IPropertyGetterSetup<T>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the property's getter is accessed.
	/// </summary>
	IPropertySetupCallbackBuilder<T> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the property's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives the value of the property as single parameter.
	/// </remarks>
	IPropertySetupCallbackBuilder<T> Do(Action<T> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the property's getter is accessed.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter and the value of the property as second
	///     parameter.
	/// </remarks>
	IPropertySetupCallbackBuilder<T> Do(Action<int, T> callback);
}

/// <summary>
///     Interface for setting up a property setter with fluent syntax.
/// </summary>
public interface IPropertySetterSetup<T>
{
	/// <summary>
	///     Registers a callback to be invoked whenever the property's value is set.
	/// </summary>
	IPropertySetupCallbackBuilder<T> Do(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the property's value is set.
	/// </summary>
	/// <remarks>
	///     The callback receives the value the property is set to as single parameter.
	/// </remarks>
	IPropertySetupCallbackBuilder<T> Do(Action<T> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the property's value is set.
	/// </summary>
	/// <remarks>
	///     The callback receives an incrementing access counter as first parameter and the value the property is set to as
	///     second parameter.
	/// </remarks>
	IPropertySetupCallbackBuilder<T> Do(Action<int, T> callback);
}

/// <summary>
///     Interface for setting up a property with fluent syntax.
/// </summary>
public interface IPropertySetup<T>
{
	/// <summary>
	///     Sets up callbacks on the getter.
	/// </summary>
	IPropertyGetterSetup<T> OnGet { get; }

	/// <summary>
	///     Sets up callbacks on the setter.
	/// </summary>
	IPropertySetterSetup<T> OnSet { get; }

	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	///     <para />
	///     If not specified, use <see cref="MockBehavior.SkipBaseClass" />.
	/// </remarks>
	IPropertySetup<T> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Register the property to have a setup without a specific value.
	/// </summary>
	/// <remarks>
	///     This is necessary when the mock uses <see cref="MockBehavior.ThrowWhenNotSetup" />.
	/// </remarks>
	IPropertySetup<T> Register();

	/// <summary>
	///     Initializes the property with the given <paramref name="value" />.
	/// </summary>
	IPropertySetup<T> InitializeWith(T value);

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this property.
	/// </summary>
	IPropertySetupReturnBuilder<T> Returns(T returnValue);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	IPropertySetupReturnBuilder<T> Returns(Func<T> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	IPropertySetupReturnBuilder<T> Returns(Func<T, T> callback);

	/// <summary>
	///     Registers a <typeparamref name="TException" /> to throw when the property is read.
	/// </summary>
	IPropertySetupReturnBuilder<T> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the property is read.
	/// </summary>
	IPropertySetupReturnBuilder<T> Throws(Exception exception);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	IPropertySetupReturnBuilder<T> Throws(Func<Exception> callback);

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	IPropertySetupReturnBuilder<T> Throws(Func<T, Exception> callback);
}

/// <summary>
///     Interface for setting up a property with fluent syntax.
/// </summary>
public interface IPropertySetupCallbackBuilder<T> : IPropertySetupCallbackWhenBuilder<T>
{
	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	IPropertySetupCallbackWhenBuilder<T> When(Func<int, bool> predicate);

	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	IPropertySetupCallbackWhenBuilder<T> InParallel();
}

/// <summary>
///     Interface for setting up a property with fluent syntax.
/// </summary>
public interface IPropertySetupCallbackWhenBuilder<T> : IPropertySetup<T>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IPropertySetupCallbackBuilder{T}.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IPropertySetupCallbackWhenBuilder<T> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IPropertySetupCallbackBuilder{T}.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IPropertySetup<T> Only(int times);
}

/// <summary>
///     Interface for setting up a return/throw builder for a property with fluent syntax.
/// </summary>
public interface IPropertySetupReturnBuilder<T> : IPropertySetupReturnWhenBuilder<T>
{
	/// <summary>
	///     Limits the return/throw to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	IPropertySetupReturnWhenBuilder<T> When(Func<int, bool> predicate);
}

/// <summary>
///     Interface for setting up a when builder for returns/throws for a property with fluent syntax.
/// </summary>
public interface IPropertySetupReturnWhenBuilder<T> : IPropertySetup<T>
{
	/// <summary>
	///     Repeats the return/throw for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IPropertySetupReturnBuilder{T}.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IPropertySetupReturnWhenBuilder<T> For(int times);

	/// <summary>
	///     Deactivates the return/throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IPropertySetupReturnBuilder{T}.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IPropertySetup<T> Only(int times);
}
