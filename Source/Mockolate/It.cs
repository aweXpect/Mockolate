using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
/// <summary>
///     Specify a matching condition for a parameter.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
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
	[DebuggerNonUserCode]
#endif
	private abstract class TypedMatch<T> : IParameter<T>, IParameter, ITypedParameter<T>
	{
		private List<Action<T>>? _callbacks;

		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter
		///     of type <typeparamref name="T" />; otherwise <see langword="false" />.
		/// </returns>
		/// <inheritdoc cref="IParameter.Matches(INamedParameterValue)" />
		public virtual bool Matches(INamedParameterValue value)
		{
			if (value.TryGetValue(out T typedValue))
			{
				return Matches(typedValue);
			}

			return false;
		}

		/// <inheritdoc cref="IParameter.InvokeCallbacks(INamedParameterValue)" />
		public void InvokeCallbacks(INamedParameterValue value)
		{
			if (_callbacks is not null && value.TryGetValue(out T typedValue))
			{
				_callbacks.ForEach(a => a.Invoke(typedValue));
			}
		}

		public IParameter<T> Do(Action<T> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		/// <inheritdoc cref="ITypedParameter{T}.MatchesValue" />
		bool ITypedParameter<T>.MatchesValue(string name, T value) => Matches(value);

		/// <summary>
		///     Verifies the expectation for the <paramref name="value" />.
		/// </summary>
		protected abstract bool Matches(T value);
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
