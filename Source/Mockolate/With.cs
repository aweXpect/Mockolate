using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mockolate.Internals;
using System.Xml.Linq;

#if NET8_0_OR_GREATER
using System.Numerics;
#endif

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
/// <summary>
///     Specify a matching condition for a method parameter.
/// </summary>
public class With
{
	private With()
	{
		// Prevent instantiation.
	}

	/// <summary>
	///     Matches any parameter of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>Also matches, if the method parameter is <see langword="null" />.</remarks>
	public static Parameter<T> Any<T>()
		=> new AnyParameter<T>();

	/// <summary>
	///     Matches any parameter combination.
	/// </summary>
	public static Parameters AnyParameterCombination()
		=> new AnyParameters();

	/// <summary>
	///     Matches a parameter of type <typeparamref name="T" /> that satisfies the <paramref name="predicate" />.
	/// </summary>
	public static Parameter<T> Matching<T>(Func<T, bool> predicate,
		[CallerArgumentExpression("predicate")] string doNotPopulateThisValue = "")
		=> new PredicateParameter<T>(predicate, doNotPopulateThisValue);

	/// <summary>
	///     Matches any parameter that is <see langword="null" />.
	/// </summary>
	public static Parameter<T> Null<T>()
		=> new NullParameter<T>();

	/// <summary>
	///     Matches any <see langword="out" /> parameter of type <typeparamref name="T" /> and
	///     uses the <paramref name="setter" /> to set the value when the method is invoked.
	/// </summary>
	public static OutParameter<T> Out<T>(Func<T> setter,
		[CallerArgumentExpression("setter")] string doNotPopulateThisValue = "")
		=> new(setter, doNotPopulateThisValue);

	/// <summary>
	///     Matches any <see langword="out" /> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static InvokedOutParameter<T> Out<T>()
		=> new();

	/// <summary>
	///     Matches any <see langword="ref" /> parameter of type <typeparamref name="T" /> and
	///     uses the <paramref name="setter" /> to set the value when the method is invoked.
	/// </summary>
	public static RefParameter<T> Ref<T>(Func<T, T> setter,
		[CallerArgumentExpression("setter")] string doNotPopulateThisValue = "")
		=> new(_ => true, setter, null, doNotPopulateThisValue);

	/// <summary>
	///     Matches a <see langword="ref" /> parameter of type <typeparamref name="T" /> that satisfies the
	///     <paramref name="predicate" /> and
	///     uses the <paramref name="setter" /> to set the value when the method is invoked.
	/// </summary>
	public static RefParameter<T> Ref<T>(Func<T, bool> predicate, Func<T, T> setter,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue1 = "",
		[CallerArgumentExpression("setter")] string doNotPopulateThisValue2 = "")
		=> new(predicate, setter, doNotPopulateThisValue1, doNotPopulateThisValue2);

	/// <summary>
	///     Matches any <see langword="ref" /> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static InvokedRefParameter<T> Ref<T>()
		=> new();

	/// <summary>
	///     Matches a parameter that is equal to <paramref name="value" />.
	/// </summary>
	public static Parameter<T> Value<T>(T value,
		[CallerArgumentExpression(nameof(value))] string doNotPopulateThisValue = "")
		=> new ParameterEquals<T>(value, doNotPopulateThisValue);

	/// <summary>
	///     Matches a parameter that is equal to <paramref name="value" /> according to the <paramref name="comparer" />.
	/// </summary>
	public static Parameter<T> Value<T>(T value, IEqualityComparer<T> comparer,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue1 = "",
		[CallerArgumentExpression(nameof(comparer))]
		string doNotPopulateThisValue2 = "")
		=> new ParameterEquals<T>(value, doNotPopulateThisValue1, comparer, doNotPopulateThisValue2);

#if NET8_0_OR_GREATER
	/// <summary>
	///     Matches a numeric parameter that is between <paramref name="minimum"/>�
	/// </summary>
	/// <remarks>
	///     By default, the comparison is inclusive of the <paramref name="minimum"/> and maximum values.
	/// </remarks>
	public static BetweenParameter<T>.Builder ValueBetween<T>(T minimum,
		[CallerArgumentExpression(nameof(minimum))] string doNotPopulateThisValue = "")
		where T : INumber<T>
		=> new BetweenParameter<T>.Builder(minimum, doNotPopulateThisValue);

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> to be between <paramref name="minimum"/> and <paramref name="maximum"/> inclusive.
	/// </summary>
	public class BetweenParameter<T>(T minimum, T maximum, string minimumExpression, string maximumExpression) : Parameter<T>
		where T : INumber<T>
	{
		/// <summary>
		///     Flag indicating whether the range includes its boundary values.
		/// </summary>
		public bool IsInclusive { get; private set; } = true;

		/// <summary>
		///     The minimum value of the range.
		/// </summary>
		public T Minimum { get; } = minimum;

		/// <summary>
		///     The maximum value of the range.
		/// </summary>
		public T Maximum { get; } = maximum;

		/// <summary>
		///     Excludes the <see cref="Minimum"/> and <see cref="Maximum"/> from the range.
		/// </summary>
		public BetweenParameter<T> Exclusive()
		{
			IsInclusive = false;
			return this;
		}

		/// <inheritdoc cref="Parameter{T}.Matches(T)" />
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
			string baseString = $"With.ValueBetween<{typeof(T).FormatType()}>({minimumExpression}).And({maximumExpression})";
			return IsInclusive ? baseString : baseString + ".Exclusive()";
		}

		/// <summary>
		///     The builder for a <see cref="BetweenParameter{T}"/>.
		/// </summary>
		public class Builder(T minimum, string minimumExpression)
		{
			/// <summary>
			///     �and <paramref name="maximum"/>.
			/// </summary>
			public BetweenParameter<T> And(T maximum,
				[CallerArgumentExpression(nameof(maximum))] string doNotPopulateThisValue = "")
			{
				if (maximum < minimum)
				{
					throw new ArgumentOutOfRangeException(nameof(maximum), "The maximum must be greater than or equal to the minimum.");
				}

				return new BetweenParameter<T>(minimum, maximum, minimumExpression, doNotPopulateThisValue);
			}
		}
	}
#endif

#pragma warning disable S1694 // Convert this 'abstract' class to an interface.
	/// <summary>
	///     Matches the method parameters against an expectation.
	/// </summary>
	public abstract class Parameters
	{
		/// <summary>
		///     Checks if the <paramref name="values" /> match the expected parameters.
		/// </summary>
		public abstract bool Matches(object?[] values);
	}

	/// <summary>
	///     Matches a method parameter against an expectation.
	/// </summary>
	public abstract class Parameter
	{
		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter; otherwise
		///     <see langword="false" />.
		/// </returns>
		public abstract bool Matches(object? value);
	}
#pragma warning restore S1694 // Convert this 'abstract' class to an interface.

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	public abstract class Parameter<T> : Parameter
	{
		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter
		///     of type <typeparamref name="T" />; otherwise <see langword="false" />.
		/// </returns>
		public override bool Matches(object? value)
		{
			if (value is T typedValue)
			{
				return Matches(typedValue);
			}

			return value is null && Matches(default!);
		}

		/// <summary>
		///     Verifies the expectation for the <paramref name="value" />.
		/// </summary>
		protected abstract bool Matches(T value);

		/// <summary>
		///     Implicitly converts to a <see cref="Parameter{T}" /> that compares the <paramref name="value" /> for equality.
		/// </summary>
		public static implicit operator Parameter<T>(T value)
		{
			return new ParameterEquals<T>(value, GetValueExpression(value));
		}

		private static string GetValueExpression(T value)
		{
			if (value is string stringValue)
			{
				return $"\"{stringValue.Replace("\"", "\\\"")}\"";
			}

			return value?.ToString() ?? "null";
		}
	}

	/// <summary>
	///     Matches an <see langword="out" /> parameter against an expectation.
	/// </summary>
	public class OutParameter<T>(Func<T> setter, string setterExpression) : Parameter
	{
		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter
		///     of type <typeparamref name="T" />; otherwise <see langword="false" />.
		/// </returns>
		public override bool Matches(object? value) => true;

		/// <summary>
		///     Retrieves the value to which the <see langword="out" /> parameter should be set.
		/// </summary>
		public T GetValue() => setter();

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"With.Out<{typeof(T).FormatType()}>({setterExpression})";
	}

#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches any <see langword="out" /> parameter.
	/// </summary>
	public class InvokedOutParameter<T> : Parameter
	{
		/// <summary>
		///     Matches any <paramref name="value" />.
		/// </summary>
		public override bool Matches(object? value) => true;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"With.Out<{typeof(T).FormatType()}>()";
	}
#pragma warning restore S2326 // Unused type parameters should be removed

	/// <summary>
	///     Matches a method <see langword="ref" /> parameter against an expectation.
	/// </summary>
	public class RefParameter<T>(
		Func<T, bool> predicate,
		Func<T, T> setter,
		string? predicateExpression,
		string setterExpression) : Parameter
	{
		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter
		///     of type <typeparamref name="T" />; otherwise <see langword="false" />.
		/// </returns>
		public override bool Matches(object? value) => value is T typedValue && predicate(typedValue);

		/// <summary>
		///     Retrieves the value to which the <see langword="ref" /> parameter should be set.
		/// </summary>
		public T GetValue(T value) => setter(value);

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"With.Ref<{typeof(T).FormatType()}>({(predicateExpression is null ? "" : $"{predicateExpression}, ")}{setterExpression})";
	}

#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches a method <see langword="out" /> parameter against an expectation.
	/// </summary>
	public class InvokedRefParameter<T> : Parameter
	{
		/// <summary>
		///     Matches any <paramref name="value" />.
		/// </summary>
		public override bool Matches(object? value) => true;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"With.Ref<{typeof(T).FormatType()}>()";
	}
#pragma warning restore S2326 // Unused type parameters should be removed

	/// <summary>
	///     A named <see cref="Parameter" />.
	/// </summary>
	/// <param name="Name">The name of the <paramref name="Parameter" />.</param>
	/// <param name="Parameter">The actual <see cref="Parameter" />.</param>
	public record NamedParameter(string Name, Parameter Parameter)
	{
		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"{Parameter} {Name}";
	}

	private sealed class AnyParameters : Parameters
	{
		/// <inheritdoc cref="Parameters.Matches(object?[])" />
		public override bool Matches(object?[] values)
			=> true;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"With.AnyParameterCombination()";
	}

	private sealed class AnyParameter<T> : Parameter<T>
	{
		protected override bool Matches(T value) => true;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"With.Any<{typeof(T).FormatType()}>()";
	}

	private sealed class NullParameter<T> : Parameter<T>
	{
		protected override bool Matches(T value) => value is null;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"With.Null<{typeof(T).FormatType()}>()";
	}

	private sealed class ParameterEquals<T> : Parameter<T>
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
				return $"With.Value({_valueExpression}, {_comparerExpression})";
			}

			return _valueExpression;
		}
	}

	private sealed class PredicateParameter<T>(Func<T, bool> predicate, string predicateExpression) : Parameter<T>
	{
		protected override bool Matches(T value) => predicate(value);
		public override string ToString() => $"With.Matching<{typeof(T).FormatType()}>({predicateExpression})";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
