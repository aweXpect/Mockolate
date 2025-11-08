using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mockolate.Internals;
using Mockolate.Match;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Parameter
{
	/// <summary>
	///     Matches a parameter that is equal to <paramref name="value" />.
	/// </summary>
	public static IParameter<T> With<T>(T value,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue = "")
		=> new ParameterEquals<T>(value, doNotPopulateThisValue);

	/// <summary>
	///     Matches a parameter that is equal to <paramref name="value" /> according to the <paramref name="comparer" />.
	/// </summary>
	public static IParameter<T> With<T>(T value, IEqualityComparer<T> comparer,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue1 = "",
		[CallerArgumentExpression(nameof(comparer))]
		string doNotPopulateThisValue2 = "")
		=> new ParameterEquals<T>(value, doNotPopulateThisValue1, comparer, doNotPopulateThisValue2);

	/// <summary>
	///     Matches a parameter of type <typeparamref name="T" /> that satisfies the <paramref name="predicate" />.
	/// </summary>
	public static IParameter<T> With<T>(Func<T, bool> predicate,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue = "")
		=> new PredicateParameter<T>(predicate, doNotPopulateThisValue);

	private sealed class ParameterEquals<T> : TypedParameter<T>
	{
		private readonly IEqualityComparer<T>? _comparer;
		private readonly string? _comparerExpression;
		private readonly T _value;
		private readonly string _valueExpression;

		public ParameterEquals(T value, string valueExpression, IEqualityComparer<T>? comparer = null,
			string? comparerExpression = null)
		{
			_value = value;
			_valueExpression = valueExpression;
			_comparer = comparer;
			_comparerExpression = comparerExpression;
		}

		protected override bool Matches(T value)
		{
			if (_comparer is not null)
			{
				return _comparer.Equals(value, _value);
			}

			return EqualityComparer<T>.Default.Equals(value, _value);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string? ToString()
		{
			if (_comparer is not null)
			{
				return $"With({_valueExpression}, {_comparerExpression})";
			}

			return _valueExpression;
		}
	}

	private sealed class PredicateParameter<T>(Func<T, bool> predicate, string predicateExpression) : TypedParameter<T>
	{
		protected override bool Matches(T value) => predicate(value);
		public override string ToString() => $"With<{typeof(T).FormatType()}>({predicateExpression})";
	}
}
#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
