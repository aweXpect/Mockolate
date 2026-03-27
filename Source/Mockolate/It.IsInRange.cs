using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches any parameter that is within the specified range.
	/// </summary>
	public static InRangeMatch<T> IsInRange<T>(T minimum, T maximum,
		[CallerArgumentExpression("minimum")] string doNotPopulateThisValue1 = "",
		[CallerArgumentExpression("maximum")] string doNotPopulateThisValue2 = "")
		where T : IComparable<T>
		=> new InRangeMatch<T>(minimum, maximum, doNotPopulateThisValue1, doNotPopulateThisValue2);

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> to be in a given range.
	/// </summary>
	[DebuggerNonUserCode]
	public class InRangeMatch<T> : ParameterMatcher<T>
		where T : IComparable<T>
	{
		private readonly T _maximum;
		private readonly string _maximumExpression;
		private readonly T _minimum;
		private readonly string _minimumExpression;
		private bool _includeBounds = true;

		/// <inheritdoc cref="InRangeMatch{T}" />
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

		/// <summary>
		///     Exclude the minimum and maximum of the range.
		/// </summary>
		public InRangeMatch<T> Exclusive()
		{
			_includeBounds = false;
			return this;
		}

		/// <summary>
		///     Include the minimum and maximum of the range.
		/// </summary>
		/// <remarks>
		///     This is the default behavior.
		/// </remarks>
		public InRangeMatch<T> Inclusive()
		{
			_includeBounds = true;
			return this;
		}

		/// <inheritdoc cref="ParameterMatcher{T}.Matches(T)" />
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
