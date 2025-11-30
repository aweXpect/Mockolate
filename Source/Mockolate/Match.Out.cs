using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mockolate.Internals;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches any <see langword="out" /> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static IVerifyOutParameter<T> Out<T>()
		=> new InvokedOutParameterMatch<T>();

	/// <summary>
	///     Matches any <see langword="out" /> parameter of type <typeparamref name="T" /> and
	///     uses the <paramref name="setter" /> to set the value when the method is invoked.
	/// </summary>
	public static IOutParameter<T> Out<T>(Func<T> setter,
		[CallerArgumentExpression("setter")] string doNotPopulateThisValue = "")
		=> new OutParameterMatch<T>(setter, doNotPopulateThisValue);

	/// <summary>
	///     Matches any <see langword="out" /> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static IOutParameter<T> AnyOut<T>()
		=> new AnyOutParameterMatch<T>();

	/// <summary>
	///     Matches an <see langword="out" /> parameter against an expectation.
	/// </summary>
	private sealed class OutParameterMatch<T>(Func<T> setter, string setterExpression) : TypedOutMatch<T>
	{
		/// <inheritdoc cref="IOutParameter{T}.GetValue(Func{T})" />
		public override T GetValue(Func<T> defaultValue) => setter();

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Out<{typeof(T).FormatType()}>({setterExpression})";
	}

	/// <summary>
	///     Matches an <see langword="out" /> parameter against an expectation.
	/// </summary>
	private sealed class AnyOutParameterMatch<T> : TypedOutMatch<T>
	{
		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"AnyOut<{typeof(T).FormatType()}>()";
	}

	/// <summary>
	///     Matches any <see langword="out" /> parameter.
	/// </summary>
	private sealed class InvokedOutParameterMatch<T> : IVerifyOutParameter<T>, IParameter
	{
		/// <inheritdoc cref="IParameter.Matches(object?)" />
		public bool Matches(object? value) => true;

		/// <inheritdoc cref="IParameter.InvokeCallbacks(object?)" />
		public void InvokeCallbacks(object? value)
		{
			// Do nothing
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Out<{typeof(T).FormatType()}>()";
	}

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	private abstract class TypedOutMatch<T> : IOutParameter<T>, IParameter
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

		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter
		///     of type <typeparamref name="T" />; otherwise <see langword="false" />.
		/// </returns>
		public bool Matches(object? value)
			=> value is T or null;

		/// <inheritdoc cref="IParameter.InvokeCallbacks(object?)" />
		public void InvokeCallbacks(object? value)
		{
			if (TryCast(value, out T typedValue) && _callbacks is not null)
			{
				_callbacks.ForEach(a => a.Invoke(typedValue));
			}
		}
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
