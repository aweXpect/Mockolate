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
	TResult InvokeGetter<TResult>(IInteraction invocation, MockBehavior behavior, Func<TResult>? defaultValueGenerator);

	/// <summary>
	///     Checks if the <paramref name="propertyAccess" /> matches the setup.
	/// </summary>
	bool Matches(PropertyAccess propertyAccess);

	/// <summary>
	///     Gets a flag indicating if the base class implementation should be called, and its return values used as default
	///     values.
	/// </summary>
	/// <remarks>
	///     When not explicitly set on the <see cref="IPropertySetup{T}" />, returns <see langword="null" />.
	/// </remarks>
	bool? CallBaseClass();

	/// <summary>
	///     Initialize the <see cref="IPropertySetup{T}" /> with the <paramref name="value" />.
	/// </summary>
	void InitializeWith(object? value);
}

/// <summary>
///     Interface for setting up a property with fluent syntax.
/// </summary>
public interface IPropertySetup<T>
{
	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     If not specified, use <see cref="MockBehavior.CallBaseClass" />.
	/// </remarks>
	IPropertySetup<T> CallingBaseClass(bool callBaseClass = true);

	/// <summary>
	///     Initializes the property with the given <paramref name="value" />.
	/// </summary>
	IPropertySetup<T> InitializeWith(T value);

	/// <summary>
	///     Registers a callback to be invoked whenever the property's getter is accessed.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value is read.
	/// </remarks>
	IPropertySetupCallbackBuilder<T> OnGet(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the property's getter is accessed.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value is read.
	/// </remarks>
	IPropertySetupCallbackBuilder<T> OnGet(Action<T> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the property's getter is accessed.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value is read.
	/// </remarks>
	IPropertySetupCallbackBuilder<T> OnGet(Action<int, T> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the property's value is set. The callback receives the new value being
	///     set.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value changes.
	/// </remarks>
	IPropertySetupCallbackBuilder<T> OnSet(Action callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the property's value is set. The callback receives the new value being
	///     set.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value changes.
	/// </remarks>
	IPropertySetupCallbackBuilder<T> OnSet(Action<T, T> callback);

	/// <summary>
	///     Registers a callback to be invoked whenever the property's value is set. The callback receives the new value being
	///     set.
	/// </summary>
	/// <remarks>
	///     Use this method to perform custom logic or side effects whenever the property's value changes.
	/// </remarks>
	IPropertySetupCallbackBuilder<T> OnSet(Action<int, T, T> callback);

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
	///     Registers an <typeparamref name="TException" /> to throw when the property is read.
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
public interface IPropertySetupCallbackBuilder<T> : IPropertySetupWhenBuilder<T>
{
	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	IPropertySetupWhenBuilder<T> When(Func<int, bool> predicate);
}

/// <summary>
///     Interface for setting up a property with fluent syntax.
/// </summary>
public interface IPropertySetupWhenBuilder<T> : IPropertySetup<T>
{
	/// <summary>
	///     Limits the callback to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IPropertySetupCallbackBuilder{T}.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IPropertySetup<T> For(int times);
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
	///     Limits the return/throw to only execute for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IPropertySetupReturnBuilder{T}.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	IPropertySetup<T> For(int times);
}
