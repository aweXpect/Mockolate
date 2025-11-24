using System;
using System.Runtime.CompilerServices;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches any parameter that is within the specified range.
	/// </summary>
	public static IInRangeParameter<T> InRange<T>(T minimum, T maximum,
		[CallerArgumentExpression("minimum")] string doNotPopulateThisValue1 = "",
		[CallerArgumentExpression("maximum")] string doNotPopulateThisValue2 = "")
		where T : IComparable<T>
		=> new InRangeMatch<T>(minimum, maximum, doNotPopulateThisValue1, doNotPopulateThisValue2);

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> to be in a given range.
	/// </summary>
	public interface IInRangeParameter<out T> : IParameter<T>
	{
		/// <summary>
		///     Exclude the minimum and maximum of the range.
		/// </summary>
		IParameter<T> Exclusive();

		/// <summary>
		///     Include the minimum and maximum of the range.
		/// </summary>
		/// <remarks>
		///     This is the default behavior.
		/// </remarks>
		IParameter<T> Inclusive();
	}

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
					"The maximum value must be greater than or equal to the minimum value.");
			}

			_minimum = minimum;
			_maximum = maximum;
			_minimumExpression = minimumExpression;
			_maximumExpression = maximumExpression;
		}

		/// <inheritdoc cref="IInRangeParameter{T}.Exclusive()" />
		public IParameter<T> Exclusive()
		{
			_includeBounds = false;
			return this;
		}

		/// <inheritdoc cref="IInRangeParameter{T}.Inclusive()" />
		public IParameter<T> Inclusive()
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
			? $"InRange({_minimumExpression}, {_maximumExpression})"
			: $"InRange({_minimumExpression}, {_maximumExpression}).Exclusive()";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
