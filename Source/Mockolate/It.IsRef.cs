using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches any <see langword="ref" /> parameter of type <typeparamref name="T" /> and replaces its value with
	///     the result of <paramref name="setter" /> when the method is invoked.
	/// </summary>
	/// <remarks>
	///     <paramref name="setter" /> receives the caller's current value and returns the new one; this is how
	///     Mockolate mocks a method that mutates a ref argument. Use <see cref="IsRef{T}(Func{T, bool}, string)" />
	///     when you only want to match (without mutating), or <see cref="IsRef{T}()" /> for verification.
	/// </remarks>
	/// <typeparam name="T">The ref-parameter's type.</typeparam>
	/// <param name="setter">Factory that takes the caller's current value and returns the replacement value.</param>
	/// <param name="doNotPopulateThisValue">Do not populate - captured automatically by the compiler.</param>
	/// <returns>An <see cref="IRefParameter{T}" /> that mutates the caller's ref-variable via <paramref name="setter" />.</returns>
	public static IRefParameter<T> IsRef<T>(Func<T, T> setter,
		[CallerArgumentExpression("setter")] string doNotPopulateThisValue = "")
		=> new RefParameterMatch<T>(_ => true, setter, null, doNotPopulateThisValue);

	/// <summary>
	///     Matches a <see langword="ref" /> parameter of type <typeparamref name="T" /> whose current value satisfies
	///     <paramref name="predicate" />, and replaces its value with the result of <paramref name="setter" />.
	/// </summary>
	/// <remarks>
	///     Combine a predicate gate with a value mutation. Both source expressions are captured by the compiler and
	///     appear in failure messages.
	/// </remarks>
	/// <typeparam name="T">The ref-parameter's type.</typeparam>
	/// <param name="predicate">The predicate evaluated against the caller's current value.</param>
	/// <param name="setter">Factory that takes the caller's current value and returns the replacement value.</param>
	/// <param name="doNotPopulateThisValue1">Do not populate - captured automatically by the compiler.</param>
	/// <param name="doNotPopulateThisValue2">Do not populate - captured automatically by the compiler.</param>
	/// <returns>An <see cref="IRefParameter{T}" /> that matches when <paramref name="predicate" /> is satisfied and mutates via <paramref name="setter" />.</returns>
	public static IRefParameter<T> IsRef<T>(Func<T, bool> predicate, Func<T, T> setter,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue1 = "",
		[CallerArgumentExpression("setter")] string doNotPopulateThisValue2 = "")
		=> new RefParameterMatch<T>(predicate, setter, doNotPopulateThisValue1, doNotPopulateThisValue2);

	/// <summary>
	///     Matches a <see langword="ref" /> parameter of type <typeparamref name="T" /> whose current value satisfies
	///     <paramref name="predicate" />, without replacing it.
	/// </summary>
	/// <remarks>
	///     Useful when you want to assert on the in-value of a ref argument (via <c>Verify</c>) without mutating it.
	/// </remarks>
	/// <typeparam name="T">The ref-parameter's type.</typeparam>
	/// <param name="predicate">The predicate evaluated against the caller's current value.</param>
	/// <param name="doNotPopulateThisValue">Do not populate - captured automatically by the compiler.</param>
	/// <returns>An <see cref="IRefParameter{T}" /> that matches when <paramref name="predicate" /> is satisfied and does not mutate the ref-variable.</returns>
	public static IRefParameter<T> IsRef<T>(Func<T, bool> predicate,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue = "")
		=> new RefParameterMatch<T>(predicate, null, doNotPopulateThisValue, null);

	/// <summary>
	///     Placeholder matcher for a <see langword="ref" /> parameter of type <typeparamref name="T" />, intended
	///     for use in <c>Verify</c>.
	/// </summary>
	/// <remarks>
	///     Accepts any ref-argument without constraint. For <c>Setup</c>, use one of the overloads that accept a
	///     <c>setter</c> to mutate the caller's value.
	/// </remarks>
	/// <typeparam name="T">The ref-parameter's type.</typeparam>
	/// <returns>An <see cref="IVerifyRefParameter{T}" /> that matches any ref-argument.</returns>
	public static IVerifyRefParameter<T> IsRef<T>()
		=> new InvokedRefParameterMatch<T>();

	/// <summary>
	///     Matches any <see langword="ref" /> parameter of type <typeparamref name="T" /> without replacing its value.
	/// </summary>
	/// <remarks>
	///     Unlike <see cref="IsRef{T}()" /> (which is only for verification), <see cref="IsAnyRef{T}" /> returns an
	///     <see cref="IRefParameter{T}" /> usable in <c>Setup</c>. Use it when the method has a <see langword="ref" />
	///     argument you don't care to inspect or mutate.
	/// </remarks>
	/// <typeparam name="T">The ref-parameter's type.</typeparam>
	/// <returns>An <see cref="IRefParameter{T}" /> that matches any ref-argument and leaves it unchanged.</returns>
	public static IRefParameter<T> IsAnyRef<T>()
		=> new AnyRefParameterMatch<T>();

	/// <summary>
	///     Matches a method <see langword="ref" /> parameter against an expectation.
	/// </summary>
#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class RefParameterMatch<T>(
		Func<T, bool> predicate,
		Func<T, T>? setter,
		string? predicateExpression,
		string? setterExpression) : TypedRefMatch<T>
	{
		/// <inheritdoc cref="TypedRefMatch{T}.Matches(T)" />
		protected override bool Matches(T value)
			=> predicate(value);

		/// <inheritdoc cref="IRefParameter{T}.GetValue(T)" />
		public override T GetValue(T value)
		{
			if (setter is null)
			{
				return value;
			}

			return setter(value);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> (predicateExpression is not null, setterExpression is not null) switch
			{
				(true, true) => $"It.IsRef<{typeof(T).FormatType()}>({predicateExpression}, {setterExpression})",
				(true, false) => $"It.IsRef<{typeof(T).FormatType()}>({predicateExpression})",
				(false, _) => $"It.IsRef<{typeof(T).FormatType()}>({setterExpression})",
			};
	}

	/// <summary>
	///     Matches any method <see langword="ref" /> parameter.
	/// </summary>
#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class AnyRefParameterMatch<T> : TypedRefMatch<T>
	{
		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"It.IsAnyRef<{typeof(T).FormatType()}>()";

		/// <inheritdoc cref="TypedRefMatch{T}.Matches(T)" />
		protected override bool Matches(T value)
			=> true;
	}

	/// <summary>
	///     Matches a method <see langword="out" /> parameter against an expectation.
	/// </summary>
#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class InvokedRefParameterMatch<T> : IVerifyRefParameter<T>, IParameterMatch<T>
	{
		/// <inheritdoc cref="IParameterMatch{T}.InvokeCallbacks(T)" />
		bool IParameterMatch<T>.Matches(T value)
			=> true;

		/// <inheritdoc cref="IParameterMatch{T}.InvokeCallbacks(T)" />
		void IParameterMatch<T>.InvokeCallbacks(T value)
		{
			// Do nothing
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"It.IsRef<{typeof(T).FormatType()}>()";
	}

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private abstract class TypedRefMatch<T> : IRefParameter<T>, IParameterMatch<T>
	{
		private List<Action<T>>? _callbacks;

		/// <inheritdoc cref="IParameterMatch{T}.InvokeCallbacks(T)" />
		bool IParameterMatch<T>.Matches(T value)
			=> Matches(value);

		/// <inheritdoc cref="IParameterMatch{T}.InvokeCallbacks(T)" />
		void IParameterMatch<T>.InvokeCallbacks(T value)
		{
			if (_callbacks is not null)
			{
				_callbacks.ForEach(a => a.Invoke(value));
			}
		}

		/// <inheritdoc cref="IRefParameter{T}.GetValue(T)" />
		public virtual T GetValue(T value)
			=> value;

		/// <inheritdoc cref="IRefParameter{T}.Do(Action{T})" />
		public IRefParameter<T> Do(Action<T> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		/// <summary>
		///     Verifies the expectation for the <paramref name="value" />.
		/// </summary>
		protected abstract bool Matches(T value);
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
