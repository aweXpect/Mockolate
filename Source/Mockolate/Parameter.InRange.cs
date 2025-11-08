using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Mockolate.Internals;
using Mockolate.Match;
using static Mockolate.Parameter;

namespace Mockolate;

public partial class Parameter
{
#if NET8_0_OR_GREATER
	/// <summary>
	///     Matches a numeric parameter that is between <paramref name="minimum"/> and <paramref name="maximum"/>.
	/// </summary>
	/// <remarks>
	///     By default, the comparison is inclusive of the <paramref name="minimum"/> and <paramref name="maximum"/> values.
	/// </remarks>
	public static IRangeParameter<T> InRange<T>(T minimum, T maximum,
		[CallerArgumentExpression(nameof(minimum))] string doNotPopulateThisValue1 = "",
		[CallerArgumentExpression(nameof(maximum))] string doNotPopulateThisValue2 = "")
		where T : INumber<T>
		=> new RangeParameter<T>(minimum, maximum, doNotPopulateThisValue1, doNotPopulateThisValue2);

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> to be between <see cref="Minimum"/> and <see cref="Maximum"/> inclusive.
	/// </summary>
	public interface IRangeParameter<T> : IParameter<T>
	{
		/// <summary>
		///     The minimum value of the range.
		/// </summary>
		T Minimum { get; }

		/// <summary>
		///     The maximum value of the range.
		/// </summary>
		T Maximum { get; }

		/// <summary>
		///     Excludes the <see cref="Minimum"/> and <see cref="Maximum"/> values from the range.
		/// </summary>
		IRangeParameter<T> Exclusive();
	}


	private sealed class RangeParameter<T> : TypedParameter<T>, IRangeParameter<T>
		where T : INumber<T>
	{
		private readonly string _minimumExpression;
		private readonly string _maximumExpression;

		public RangeParameter(T minimum, T maximum, string minimumExpression, string maximumExpression)
		{
			if (maximum < minimum)
			{
				throw new ArgumentOutOfRangeException(nameof(maximum), "The maximum must be greater than or equal to the minimum.");
			}

			_minimumExpression = minimumExpression;
			_maximumExpression = maximumExpression;
			Minimum = minimum;
			Maximum = maximum;
		}

		/// <summary>
		///     Flag indicating whether the range includes its boundary values.
		/// </summary>
		public bool IsInclusive { get; private set; } = true;

		/// <summary>
		///     The minimum value of the range.
		/// </summary>
		public T Minimum { get; }

		/// <summary>
		///     The maximum value of the range.
		/// </summary>
		public T Maximum { get; }

		/// <summary>
		///     Excludes the <see cref="Minimum"/> and <see cref="Maximum"/> from the range.
		/// </summary>
		public IRangeParameter<T> Exclusive()
		{
			IsInclusive = false;
			return this;
		}

		/// <inheritdoc cref="TypedParameter{T}.Matches(T)" />
		protected override bool Matches(T value)
		{
			if (IsInclusive)
			{
				return Minimum <= value && value <= Maximum;
			}

			return Minimum < value && value < Maximum;
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
		{
			string baseString = $"InRange<{typeof(T).FormatType()}>({_minimumExpression}, {_maximumExpression})";
			return IsInclusive ? baseString : baseString + ".Exclusive()";
		}
	}
#endif
}
