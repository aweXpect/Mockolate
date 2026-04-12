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
	private abstract class TypedMatch<T> : IParameter<T>, IParameterMatch<T>
	{
		private List<Action<T>>? _callbacks;

		/// <inheritdoc cref="IParameter{T}.Do(Action{T})" />
		IParameter<T> IParameter<T>.Do(Action<T> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		/// <inheritdoc cref="IParameterMatch{T}.InvokeCallbacks(T)" />
		void IParameterMatch<T>.InvokeCallbacks(T value)
		{
			if (_callbacks is not null)
			{
				_callbacks.ForEach(a => a.Invoke(value));
			}
		}

		/// <inheritdoc cref="IParameterMatch{T}.Matches(T)" />
		bool IParameterMatch<T>.Matches(T value)
			=> Matches(value);

		/// <summary>
		///     Verifies the expectation for the <paramref name="value" />.
		/// </summary>
		protected abstract bool Matches(T value);
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
