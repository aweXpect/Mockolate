using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches a parameter that is equal to <paramref name="value" />.
	/// </summary>
	public static IParameter<T> Is<T>(T value,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue = "")
		=> new ParameterEqualsMatch<T>(value, doNotPopulateThisValue);

	/// <summary>
	///     Matches a parameter that is equal to <paramref name="value" /> according to the <paramref name="comparer" />.
	/// </summary>
	public static IParameter<T> Is<T>(T value, IEqualityComparer<T> comparer,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue1 = "",
		[CallerArgumentExpression(nameof(comparer))]
		string doNotPopulateThisValue2 = "")
		=> new ParameterEqualsMatch<T>(value, doNotPopulateThisValue1, comparer, doNotPopulateThisValue2);

	/// <summary>
	///     Matches a parameter of type <typeparamref name="T" /> that satisfies the <paramref name="predicate" />.
	/// </summary>
	public static IParameter<T> Is<T>(Func<T, bool> predicate,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue = "")
		=> new PredicateParameterMatch<T>(predicate, doNotPopulateThisValue);

	private sealed class ParameterEqualsMatch<T>(
		T value,
		string valueExpression,
		IEqualityComparer<T>? comparer = null,
		string? comparerExpression = null)
		: TypedMatch<T>
	{
		/// <inheritdoc cref="TypedMatch{T}.Matches(T)" />
		protected override bool Matches(T value1)
		{
			if (comparer is not null)
			{
				return comparer.Equals(value1, value);
			}

			return EqualityComparer<T>.Default.Equals(value1, value);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
		{
			if (comparer is not null)
			{
				return $"It.Is({valueExpression}, {comparerExpression})";
			}

			return valueExpression;
		}
	}

	private sealed class PredicateParameterMatch<T>(Func<T, bool> predicate, string predicateExpression) : TypedMatch<T>
	{
		protected override bool Matches(T value) => predicate(value);
		public override string ToString() => $"It.Is<{typeof(T).FormatType()}>({predicateExpression})";
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
