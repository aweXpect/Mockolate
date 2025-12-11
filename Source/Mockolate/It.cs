using System;
using System.Collections.Generic;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
/// <summary>
///     Specify a matching condition for a parameter.
/// </summary>
public partial class It
{
	private It()
	{
		// Prevent instantiation.
	}

	private static bool TryCast<T>(object? value, out T typedValue)
	{
		if (value is T castValue)
		{
			typedValue = castValue;
			return true;
		}

		if (value is null && default(T) is null)
		{
			typedValue = default!;
			return true;
		}

		typedValue = default!;
		return false;
	}

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	private abstract class TypedMatch<T> : IParameter<T>, IParameter
	{
		private List<Action<T>>? _callbacks;

		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter
		///     of type <typeparamref name="T" />; otherwise <see langword="false" />.
		/// </returns>
		public bool Matches(object? value)
		{
			if (value is T typedValue)
			{
				return Matches(typedValue);
			}

			return value is null && Matches(default!);
		}

		public void InvokeCallbacks(object? value)
		{
			if (TryCast(value, out T typedValue) && _callbacks is not null)
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

		/// <summary>
		///     Verifies the expectation for the <paramref name="value" />.
		/// </summary>
		protected abstract bool Matches(T value);
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
