using System;
using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches any parameter whose value lies between <paramref name="minimum" /> and <paramref name="maximum" />
	///     according to <see cref="IComparable{T}.CompareTo" />.
	/// </summary>
	/// <remarks>
	///     Bounds are inclusive by default (<see cref="IInRangeParameter{T}.Inclusive" />); chain
	///     <see cref="IInRangeParameter{T}.Exclusive" /> to exclude them.
	///     Works on any type that implements <see cref="IComparable{T}" /> - numerics,
	///     <see cref="System.DateTime" />, <see cref="TimeSpan" />, etc.
	/// </remarks>
	/// <typeparam name="T">The declared type of the parameter.</typeparam>
	/// <param name="minimum">Lower bound of the accepted range.</param>
	/// <param name="maximum">Upper bound of the accepted range.</param>
	/// <param name="doNotPopulateThisValue1">Do not populate - captured automatically by the compiler.</param>
	/// <param name="doNotPopulateThisValue2">Do not populate - captured automatically by the compiler.</param>
	/// <returns>A parameter matcher that accepts values in the specified range.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="maximum" /> is less than <paramref name="minimum" />.</exception>
	public static IInRangeParameter<T> IsInRange<T>(T minimum, T maximum,
		[CallerArgumentExpression("minimum")] string doNotPopulateThisValue1 = "",
		[CallerArgumentExpression("maximum")] string doNotPopulateThisValue2 = "")
		where T : IComparable<T>
		=> new InRangeMatch<T>(minimum, maximum, doNotPopulateThisValue1, doNotPopulateThisValue2);

	/// <summary>
	///     A parameter matcher for an in-range constraint on values of type <typeparamref name="T" />.
	/// </summary>
	public interface IInRangeParameter<out T> : IParameterWithCallback<T>
	{
		/// <summary>
		///     Switches to exclusive bounds: values equal to the minimum or maximum are rejected.
		/// </summary>
		IParameterWithCallback<T> Exclusive();

		/// <summary>
		///     Switches to inclusive bounds: values equal to the minimum or maximum are accepted.
		/// </summary>
		/// <remarks>
		///     This is the default behavior; calling <see cref="Inclusive" /> explicitly is only needed to revert a
		///     previous <see cref="Exclusive" /> call.
		/// </remarks>
		IParameterWithCallback<T> Inclusive();
	}

#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class InRangeMatch<T> : TypedMatch<T>, IInRangeParameter<T>
		where T : IComparable<T>
	{
		private readonly T _maximum;
		private readonly string _maximumExpression;
		private readonly T _minimum;
		private readonly string _minimumExpression;
		private bool _includeBounds = true;

		public InRangeMatch(T minimum, T maximum, string minimumExpression, string maximumExpression)
		{
			if (minimum.CompareTo(maximum) > 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maximum),
					// ReSharper disable once LocalizableElement
					"The maximum value must be greater than or equal to the minimum value.");
			}

			_minimum = minimum;
			_maximum = maximum;
			_minimumExpression = minimumExpression;
			_maximumExpression = maximumExpression;
		}

		/// <inheritdoc cref="IInRangeParameter{T}.Exclusive()" />
		public IParameterWithCallback<T> Exclusive()
		{
			_includeBounds = false;
			return this;
		}

		/// <inheritdoc cref="IInRangeParameter{T}.Inclusive()" />
		public IParameterWithCallback<T> Inclusive()
		{
			_includeBounds = true;
			return this;
		}

		protected override bool Matches(T value)
		{
			if (_includeBounds)
			{
				return value.CompareTo(_minimum) >= 0 && value.CompareTo(_maximum) <= 0;
			}

			return value.CompareTo(_minimum) > 0 && value.CompareTo(_maximum) < 0;
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => _includeBounds
			? $"It.IsInRange({_minimumExpression}, {_maximumExpression})"
			: $"It.IsInRange({_minimumExpression}, {_maximumExpression}).Exclusive()";
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
