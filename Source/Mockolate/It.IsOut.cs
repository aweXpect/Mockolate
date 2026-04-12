using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches any <see langword="out" /> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static IVerifyOutParameter<T> IsOut<T>()
		=> new InvokedOutParameterMatch<T>();

	/// <summary>
	///     Matches any <see langword="out" /> parameter of type <typeparamref name="T" /> and
	///     uses the <paramref name="setter" /> to set the value when the method is invoked.
	/// </summary>
	public static IOutParameter<T> IsOut<T>(Func<T> setter,
		[CallerArgumentExpression("setter")] string doNotPopulateThisValue = "")
		=> new OutParameterMatch<T>(setter, doNotPopulateThisValue);

	/// <summary>
	///     Matches any <see langword="out" /> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static IOutParameter<T> IsAnyOut<T>()
		=> new AnyOutParameterMatch<T>();

	/// <summary>
	///     Matches an <see langword="out" /> parameter against an expectation.
	/// </summary>
#if !DEBUG
	[DebuggerNonUserCode]
#endif
	private sealed class OutParameterMatch<T>(Func<T> setter, string setterExpression) : TypedOutMatch<T>
	{
		/// <inheritdoc cref="IOutParameter{T}.GetValue(Func{T})" />
		public override T GetValue(Func<T> defaultValue) => setter();

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"It.IsOut<{typeof(T).FormatType()}>({setterExpression})";
	}

	/// <summary>
	///     Matches an <see langword="out" /> parameter against an expectation.
	/// </summary>
#if !DEBUG
	[DebuggerNonUserCode]
#endif
	private sealed class AnyOutParameterMatch<T> : TypedOutMatch<T>
	{
		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"It.IsAnyOut<{typeof(T).FormatType()}>()";
	}

	/// <summary>
	///     Matches any <see langword="out" /> parameter.
	/// </summary>
#if !DEBUG
	[DebuggerNonUserCode]
#endif
	private sealed class InvokedOutParameterMatch<T> : IVerifyOutParameter<T>, IParameterMatch<T>
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
		public override string ToString() => $"It.IsOut<{typeof(T).FormatType()}>()";
	}

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
#if !DEBUG
	[DebuggerNonUserCode]
#endif
	private abstract class TypedOutMatch<T> : IOutParameter<T>, IParameterMatch<T>
	{
		private List<Action<T>>? _callbacks;

		/// <inheritdoc cref="IOutParameter{T}.GetValue(Func{T})" />
		public virtual T GetValue(Func<T> defaultValue)
			=> defaultValue();

		/// <inheritdoc cref="IOutParameter{T}.Do(Action{T})" />
		public IOutParameter<T> Do(Action<T> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		/// <inheritdoc cref="IParameterMatch{T}.InvokeCallbacks(T)" />
		bool IParameterMatch<T>.Matches(T value)
			=> true;

		/// <inheritdoc cref="IParameterMatch{T}.InvokeCallbacks(T)" />
		void IParameterMatch<T>.InvokeCallbacks(T value)
		{
			if (_callbacks is not null)
			{
				_callbacks.ForEach(a => a.Invoke(value));
			}
		}
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
