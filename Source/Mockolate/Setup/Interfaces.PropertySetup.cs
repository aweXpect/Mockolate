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
	/// <remarks>
	///     <paramref name="invocation" /> may be <see langword="null" /> when interaction recording is skipped
	///     (see <see cref="MockBehavior.SkipInteractionRecording" />).
	/// </remarks>
	/// <typeparam name="T">The property type.</typeparam>
	/// <param name="invocation">The recorded interaction for the setter access, or <see langword="null" /> when recording is skipped.</param>
	/// <param name="value">The value being assigned to the property.</param>
	/// <param name="behavior">The mock behavior in effect for this invocation.</param>
	void InvokeSetter<T>(IInteraction? invocation, T value, MockBehavior behavior);

	/// <summary>
	///     Invokes the getter logic for the <paramref name="invocation" /> and returns the value of type
	///     <typeparamref name="TResult" />.
	/// </summary>
	/// <remarks>
	///     <paramref name="invocation" /> may be <see langword="null" /> when interaction recording is skipped
	///     (see <see cref="MockBehavior.SkipInteractionRecording" />).
	/// </remarks>
	/// <typeparam name="TResult">The property type returned by the getter.</typeparam>
	/// <param name="invocation">The recorded interaction for the getter access, or <see langword="null" /> when recording is skipped.</param>
	/// <param name="behavior">The mock behavior in effect for this invocation.</param>
	/// <param name="defaultValueGenerator">Factory producing the default value when no configured response applies.</param>
	/// <returns>The value produced by the configured setup or by <paramref name="defaultValueGenerator" />.</returns>
	TResult InvokeGetter<TResult>(IInteraction? invocation, MockBehavior behavior, Func<TResult> defaultValueGenerator);

	/// <summary>
	///     Checks if the <paramref name="propertyAccess" /> matches the setup.
	/// </summary>
	/// <param name="propertyAccess">The property access to test against this setup.</param>
	/// <returns><see langword="true" /> when the setup matches the access; otherwise <see langword="false" />.</returns>
	bool Matches(PropertyAccess propertyAccess);

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     When not explicitly set on the <see cref="IPropertySetup{T}" />, returns <see langword="null" />.
	/// </remarks>
	/// <returns>The configured override, or <see langword="null" /> when not set.</returns>
	bool? SkipBaseClass();

	/// <summary>
	///     Initialize the <see cref="IPropertySetup{T}" /> with the <paramref name="value" />.
	/// </summary>
	/// <param name="value">The initial value assigned to the property's backing field.</param>
	void InitializeWith(object? value);
}

/// <summary>
///     Fluent surface for attaching side-effects to a property's getter.
/// </summary>
/// <remarks>
///     Each <c>Do</c> registers a callback that fires on every matching read. Chain multiple <c>Do</c> calls to form
///     a sequence (cycling by default); chain <see cref="IPropertyGetterSetupCallbackBuilder{T}.InParallel" />,
///     <see cref="IPropertyGetterSetupParallelCallbackBuilder{T}.When" />,
///     <see cref="IPropertyGetterSetupCallbackWhenBuilder{T}.For" /> or
///     <see cref="IPropertyGetterSetupCallbackWhenBuilder{T}.Only" /> to control repetition and gating.
/// </remarks>
public interface IPropertyGetterSetup<T>
{
	/// <summary>
	///     Fires <paramref name="callback" /> whenever the property's getter is read.
	/// </summary>
	/// <param name="callback">The action to invoke on every matching invocation.</param>
	/// <returns>A builder for chaining repetition/gating operators.</returns>
	/// <example>
	///     <code>
	///     sut.Mock.Setup.TotalDispensed.OnGet.Do(() =&gt; Console.WriteLine("Read!"));
	///     </code>
	/// </example>
	IPropertyGetterSetupCallbackBuilder<T> Do(Action callback);

	/// <summary>
	///     Fires <paramref name="callback" /> whenever the property's getter is read, passing the property's current
	///     value.
	/// </summary>
	/// <param name="callback">The action to invoke on every matching invocation; receives the method arguments.</param>
	/// <returns>A builder for chaining repetition/gating operators.</returns>
	IPropertyGetterSetupCallbackBuilder<T> Do(Action<T> callback);

	/// <summary>
	///     Fires <paramref name="callback" /> whenever the property's getter is read, passing a zero-based read counter
	///     and the property's current value.
	/// </summary>
	/// <param name="callback">The action to invoke on every matching invocation; receives a zero-based invocation counter and the method arguments.</param>
	/// <returns>A builder for chaining repetition/gating operators.</returns>
	IPropertyGetterSetupCallbackBuilder<T> Do(Action<int, T> callback);

	/// <summary>
	///     Switches the mock's current scenario to <paramref name="scenario" /> whenever the property is read - useful
	///     to trigger state changes driven by observed reads.
	/// </summary>
	/// <param name="scenario">The name of the scenario to transition to.</param>
	/// <returns>A builder for chaining <c>.When(...)</c>, <c>.For(n)</c>, or <c>.Only(n)</c>.</returns>
	IPropertyGetterSetupParallelCallbackBuilder<T> TransitionTo(string scenario);
}

/// <summary>
///     Fluent surface for attaching side-effects to a property's setter.
/// </summary>
/// <remarks>
///     Each <c>Do</c> registers a callback that fires on every matching write. Chain multiple <c>Do</c> calls to form
///     a sequence (cycling by default); chain <see cref="IPropertySetterSetupCallbackBuilder{T}.InParallel" />,
///     <see cref="IPropertySetterSetupParallelCallbackBuilder{T}.When" />,
///     <see cref="IPropertySetterSetupCallbackWhenBuilder{T}.For" /> or
///     <see cref="IPropertySetterSetupCallbackWhenBuilder{T}.Only" /> to control repetition and gating.
/// </remarks>
public interface IPropertySetterSetup<T>
{
	/// <summary>
	///     Fires <paramref name="callback" /> whenever the property's setter runs.
	/// </summary>
	/// <param name="callback">The action to invoke on every matching invocation.</param>
	/// <returns>A builder for chaining repetition/gating operators.</returns>
	IPropertySetterSetupCallbackBuilder<T> Do(Action callback);

	/// <summary>
	///     Fires <paramref name="callback" /> whenever the property's setter runs, passing the new value.
	/// </summary>
	/// <param name="callback">The action to invoke on every matching invocation; receives the method arguments.</param>
	/// <returns>A builder for chaining repetition/gating operators.</returns>
	/// <example>
	///     <code>
	///     sut.Mock.Setup.TotalDispensed.OnSet.Do(v =&gt; Console.WriteLine($"Set to {v}"));
	///     </code>
	/// </example>
	IPropertySetterSetupCallbackBuilder<T> Do(Action<T> callback);

	/// <summary>
	///     Fires <paramref name="callback" /> whenever the property's setter runs, passing a zero-based write counter
	///     and the new value.
	/// </summary>
	/// <param name="callback">The action to invoke on every matching invocation; receives a zero-based invocation counter and the method arguments.</param>
	/// <returns>A builder for chaining repetition/gating operators.</returns>
	IPropertySetterSetupCallbackBuilder<T> Do(Action<int, T> callback);

	/// <summary>
	///     Switches the mock's current scenario to <paramref name="scenario" /> whenever the property is written -
	///     useful to trigger state changes driven by observed writes.
	/// </summary>
	/// <param name="scenario">The name of the scenario to transition to.</param>
	/// <returns>A builder for chaining <c>.When(...)</c>, <c>.For(n)</c>, or <c>.Only(n)</c>.</returns>
	IPropertySetterSetupParallelCallbackBuilder<T> TransitionTo(string scenario);
}

/// <summary>
///     Fluent surface for configuring a mocked property of type <typeparamref name="T" />.
/// </summary>
/// <remarks>
///     Reached via <c>sut.Mock.Setup.PropertyName</c>. Chain <see cref="InitializeWith" /> to back the property with a
///     mutable slot, <see cref="Returns(T)" /> / <see cref="Throws{TException}" /> to define a sequence of read
///     responses, and <see cref="OnGet" /> / <see cref="OnSet" /> to attach getter/setter callbacks. Consecutive
///     <c>Returns</c>/<c>Throws</c> calls form a sequence that cycles once exhausted - terminate with
///     <see cref="SetupExtensions.Forever{T}(IPropertySetupReturnWhenBuilder{T})" /> to freeze on the last entry, or
///     <see cref="IPropertySetupReturnWhenBuilder{T}.For(int)" /> / <see cref="IPropertySetupReturnWhenBuilder{T}.Only(int)" />
///     to control repetition.
/// </remarks>
public interface IPropertySetup<T>
{
	/// <summary>
	///     Attaches callbacks that fire whenever the property's getter is accessed.
	/// </summary>
	/// <remarks>
	///     See <see cref="IPropertyGetterSetup{T}" /> for the available <c>Do</c> overloads (parameterless, current
	///     value, counter + current value) and <c>.TransitionTo(scenario)</c>.
	/// </remarks>
	IPropertyGetterSetup<T> OnGet { get; }

	/// <summary>
	///     Attaches callbacks that fire whenever the property's setter is invoked.
	/// </summary>
	/// <remarks>
	///     See <see cref="IPropertySetterSetup{T}" /> for the available <c>Do</c> overloads (parameterless, new value,
	///     counter + new value) and <c>.TransitionTo(scenario)</c>.
	/// </remarks>
	IPropertySetterSetup<T> OnSet { get; }

	/// <summary>
	///     Overrides <see cref="MockBehavior.SkipBaseClass" /> for this property only.
	/// </summary>
	/// <remarks>
	///     Only meaningful for class mocks. When <see langword="false" /> (the default on mocks) the base-class
	///     getter/setter runs, and its return value is used as the default value for un-configured reads; when
	///     <see langword="true" /> the base-class members are skipped entirely and only the configured behavior
	///     (including <c>Returns</c>/<c>OnGet</c>/<c>OnSet</c>) is applied.
	/// </remarks>
	/// <param name="skipBaseClass">Whether to skip the base-class implementation. Defaults to <see langword="true" />.</param>
	/// <returns>The same setup instance, to allow chaining.</returns>
	IPropertySetup<T> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers the property with the mock without supplying a value.
	/// </summary>
	/// <remarks>
	///     Primarily useful under <see cref="MockBehavior.ThrowWhenNotSetup" />: it marks the property as &quot;known&quot;
	///     so a read returns the type's default value instead of throwing. Does not affect mocks with the default
	///     behavior, where un-configured reads already return defaults.
	/// </remarks>
	/// <returns>The same setup instance, to allow chaining.</returns>
	IPropertySetup<T> Register();

	/// <summary>
	///     Gives the property a mutable backing slot initialized to <paramref name="value" /> - subsequent setter
	///     invocations update the slot, and reads return the current slot value.
	/// </summary>
	/// <remarks>
	///     After <see cref="InitializeWith" /> the property behaves like a normal auto-property. Additional
	///     <c>Returns</c>/<c>Throws</c> or <c>OnSet</c>/<c>OnGet</c> calls layer extra behavior on top.
	/// </remarks>
	/// <param name="value">The initial value assigned to the property's backing field.</param>
	/// <returns>The same setup instance, to allow chaining.</returns>
	/// <example>
	///     <code>
	///     sut.Mock.Setup.TotalDispensed.InitializeWith(42);
	///     </code>
	/// </example>
	IPropertySetup<T> InitializeWith(T value);

	/// <summary>
	///     Appends <paramref name="returnValue" /> to the property's read-sequence - the next read returns this value
	///     (if no earlier sequence entry is still active).
	/// </summary>
	/// <remarks>
	///     Call <c>Returns</c>/<c>Throws</c> multiple times to build a sequence; once exhausted it cycles back to the
	///     first entry unless the last entry is followed by
	///     <see cref="SetupExtensions.Forever{T}(IPropertySetupReturnWhenBuilder{T})" />.
	/// </remarks>
	/// <param name="returnValue">The value returned on the next matching invocation.</param>
	/// <returns>A builder for chaining additional returns/throws or gating operators.</returns>
	/// <example>
	///     <code>
	///     sut.Mock.Setup.TotalDispensed
	///         .Returns(1)
	///         .Returns(2)
	///         .Throws(new Exception("boom"))
	///         .Returns(4);
	///     </code>
	/// </example>
	IPropertySetupReturnBuilder<T> Returns(T returnValue);

	/// <summary>
	///     Appends a lazy return to the property's read-sequence; <paramref name="callback" /> is invoked on each
	///     matching read and its result is returned.
	/// </summary>
	/// <param name="callback">The factory invoked on every matching invocation to produce the return value.</param>
	/// <returns>A builder for chaining additional returns/throws or gating operators.</returns>
	IPropertySetupReturnBuilder<T> Returns(Func<T> callback);

	/// <summary>
	///     Appends a lazy return that receives the property's current value and returns the new one - useful for
	///     incrementing or transforming the last-read value.
	/// </summary>
	/// <param name="callback">The factory invoked on every matching invocation; receives the method arguments and produces the return value.</param>
	/// <returns>A builder for chaining additional returns/throws or gating operators.</returns>
	/// <example>
	///     <code>
	///     sut.Mock.Setup.TotalDispensed.Returns(current =&gt; current + 10);
	///     </code>
	/// </example>
	IPropertySetupReturnBuilder<T> Returns(Func<T, T> callback);

	/// <summary>
	///     Appends an entry to the read-sequence that throws a freshly-constructed
	///     <typeparamref name="TException" /> on the next matching read.
	/// </summary>
	/// <typeparam name="TException">The exception type; a new instance is created per invocation.</typeparam>
	/// <returns>A builder for chaining additional returns/throws or gating operators.</returns>
	IPropertySetupReturnBuilder<T> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Appends an entry to the read-sequence that throws <paramref name="exception" /> on the next matching read.
	/// </summary>
	/// <param name="exception">The exception to throw on the next matching invocation.</param>
	/// <returns>A builder for chaining additional returns/throws or gating operators.</returns>
	IPropertySetupReturnBuilder<T> Throws(Exception exception);

	/// <summary>
	///     Appends an entry that invokes <paramref name="callback" /> to build the exception thrown on the next
	///     matching read - useful when the exception needs to reference the invocation state.
	/// </summary>
	/// <param name="callback">The factory that produces the exception thrown on every matching invocation.</param>
	/// <returns>A builder for chaining additional returns/throws or gating operators.</returns>
	IPropertySetupReturnBuilder<T> Throws(Func<Exception> callback);

	/// <summary>
	///     Appends an entry that invokes <paramref name="callback" /> with the property's current value to build the
	///     exception thrown on the next matching read.
	/// </summary>
	/// <param name="callback">The factory that produces the exception thrown on every matching invocation.</param>
	/// <returns>A builder for chaining additional returns/throws or gating operators.</returns>
	IPropertySetupReturnBuilder<T> Throws(Func<T, Exception> callback);
}

/// <summary>
///     Interface for setting up a property getter with fluent syntax.
/// </summary>
public interface IPropertyGetterSetupParallelCallbackBuilder<T> : IPropertyGetterSetupCallbackWhenBuilder<T>
{
	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	/// <param name="predicate">A predicate receiving a zero-based counter of how many times the method has been invoked so far.</param>
	/// <returns>A builder for chaining <c>.For(n)</c> or <c>.Only(n)</c>.</returns>
	IPropertyGetterSetupCallbackWhenBuilder<T> When(Func<int, bool> predicate);
}

/// <summary>
///     Interface for setting up a property getter with fluent syntax.
/// </summary>
public interface IPropertyGetterSetupCallbackBuilder<T> : IPropertyGetterSetupParallelCallbackBuilder<T>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	/// <returns>A builder for chaining <c>.When(...)</c>, <c>.For(n)</c>, or <c>.Only(n)</c>.</returns>
	IPropertyGetterSetupParallelCallbackBuilder<T> InParallel();
}

/// <summary>
///     Interface for setting up a property getter with fluent syntax.
/// </summary>
public interface IPropertyGetterSetupCallbackWhenBuilder<T> : IPropertySetup<T>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IPropertyGetterSetupParallelCallbackBuilder{T}.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	/// <param name="times">The number of executions before the callback cycles back to the first entry in the sequence.</param>
	/// <returns>A builder for chaining <c>.For(n)</c> or <c>.Only(n)</c>.</returns>
	IPropertyGetterSetupCallbackWhenBuilder<T> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IPropertyGetterSetupParallelCallbackBuilder{T}.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	/// <param name="times">The number of executions after which the callback stops firing.</param>
	/// <returns>The outer setup for chaining additional returns/throws.</returns>
	IPropertySetup<T> Only(int times);
}

/// <summary>
///     Interface for setting up a property setter with fluent syntax.
/// </summary>
public interface IPropertySetterSetupParallelCallbackBuilder<T> : IPropertySetterSetupCallbackWhenBuilder<T>
{
	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns true.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	/// <param name="predicate">A predicate receiving a zero-based counter of how many times the method has been invoked so far.</param>
	/// <returns>A builder for chaining <c>.For(n)</c> or <c>.Only(n)</c>.</returns>
	IPropertySetterSetupCallbackWhenBuilder<T> When(Func<int, bool> predicate);
}

/// <summary>
///     Interface for setting up a property setter with fluent syntax.
/// </summary>
public interface IPropertySetterSetupCallbackBuilder<T> : IPropertySetterSetupParallelCallbackBuilder<T>
{
	/// <summary>
	///     Runs the callback in parallel to the other callbacks.
	/// </summary>
	/// <returns>A builder for chaining <c>.When(...)</c>, <c>.For(n)</c>, or <c>.Only(n)</c>.</returns>
	IPropertySetterSetupParallelCallbackBuilder<T> InParallel();
}

/// <summary>
///     Interface for setting up a property setter with fluent syntax.
/// </summary>
public interface IPropertySetterSetupCallbackWhenBuilder<T> : IPropertySetup<T>
{
	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IPropertySetterSetupParallelCallbackBuilder{T}.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	/// <param name="times">The number of executions before the callback cycles back to the first entry in the sequence.</param>
	/// <returns>A builder for chaining <c>.For(n)</c> or <c>.Only(n)</c>.</returns>
	IPropertySetterSetupCallbackWhenBuilder<T> For(int times);

	/// <summary>
	///     Deactivates the callback after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IPropertySetterSetupParallelCallbackBuilder{T}.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	/// <param name="times">The number of executions after which the callback stops firing.</param>
	/// <returns>The outer setup for chaining additional returns/throws.</returns>
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
	/// <param name="predicate">A predicate receiving a zero-based counter of how many times the method has been invoked so far.</param>
	/// <returns>A builder for chaining <c>.For(n)</c> or <c>.Only(n)</c>.</returns>
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
	/// <param name="times">The number of executions before the callback cycles back to the first entry in the sequence.</param>
	/// <returns>A builder for chaining <c>.For(n)</c> or <c>.Only(n)</c>.</returns>
	IPropertySetupReturnWhenBuilder<T> For(int times);

	/// <summary>
	///     Deactivates the return/throw after the given number of <paramref name="times" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (
	///     <see cref="IPropertySetupReturnBuilder{T}.When(Func{int, bool})" /> evaluates to <see langword="true" />).
	/// </remarks>
	/// <param name="times">The number of executions after which the callback stops firing.</param>
	/// <returns>The outer setup for chaining additional returns/throws.</returns>
	IPropertySetup<T> Only(int times);
}
