using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
/// <summary>
///     Specify a matching condition for a parameter.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public partial class It
{
	/// <summary>
	///     This class is intentionally not static to allow adding static extension methods on <see cref="It" />.
	/// </summary>
	[ExcludeFromCodeCoverage]
	private It()
	{
		// Prevent instantiation.
	}

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private abstract class TypedMatch<T> : IParameterWithCallback<T>, IParameterMatch<T>
	{
		private List<Action<T>>? _callbacks;

		/// <inheritdoc cref="IParameterWithCallback{T}.Do(Action{T})" />
		IParameterWithCallback<T> IParameterWithCallback<T>.Do(Action<T> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		/// <inheritdoc cref="IParameterMatch{T}.InvokeCallbacks(T)" />
		void IParameterMatch<T>.InvokeCallbacks(T value)
			=> InvokeCallbacksCore(value);

		/// <inheritdoc cref="IParameterMatch{T}.Matches(T)" />
		bool IParameterMatch<T>.Matches(T value)
			=> Matches(value);

		/// <inheritdoc cref="IParameter.Matches(object?)" />
		bool IParameter.Matches(object? value)
		{
			if (value is T typedValue)
			{
				return Matches(typedValue);
			}

			if (value is null && default(T) is null)
			{
				return Matches(default!);
			}

			return MatchesOfDifferentType(value);
		}

		/// <summary>
		///     Called from <see cref="IParameter.Matches(object?)" /> when the incoming value is not of type
		///     <typeparamref name="T" /> and is not the default-null case. Returned when the matcher reached via covariant
		///     widening sees a value whose runtime type diverges from <typeparamref name="T" />.
		/// </summary>
		/// <remarks>
		///     Default: <see langword="false" /> (positive matchers — <c>Is</c>, <c>IsAny</c>, <c>IsOneOf</c>, etc. — cannot
		///     match a value that isn't even of their matched type). Override to <see langword="true" /> in negative
		///     matchers (<c>IsNot</c>, <c>IsNotOneOf</c>, <c>IsNotNull</c>) where a type mismatch also satisfies the
		///     "not equal / not null" semantic.
		/// </remarks>
		protected virtual bool MatchesOfDifferentType(object? value) => false;

		/// <inheritdoc cref="IParameter.InvokeCallbacks(object?)" />
		void IParameter.InvokeCallbacks(object? value)
		{
			if (value is T typedValue)
			{
				InvokeCallbacksCore(typedValue);
			}
			else if (value is null && default(T) is null)
			{
				InvokeCallbacksCore(default!);
			}
		}

		private void InvokeCallbacksCore(T value)
		{
			if (_callbacks is not null)
			{
				_callbacks.ForEach(a => a.Invoke(value));
			}
		}

		/// <summary>
		///     Verifies the expectation for the <paramref name="value" />.
		/// </summary>
		protected abstract bool Matches(T value);
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
