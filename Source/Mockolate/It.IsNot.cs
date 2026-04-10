using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches a parameter that is not equal to <paramref name="value" />.
	/// </summary>
	public static IIsNotParameter<T> IsNot<T>(T value,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue = "")
		=> new ParameterEqualsNotMatch<T>(value, doNotPopulateThisValue);

	/// <summary>
	///     An <see cref="IParameter{T}" /> used for equality comparison.
	/// </summary>
	public interface IIsNotParameter<out T> : IParameter<T>
	{
		/// <summary>
		///     Use the specified comparer to determine equality.
		/// </summary>
		IIsNotParameter<T> Using(IEqualityComparer<T> comparer,
			[CallerArgumentExpression(nameof(comparer))]
			string doNotPopulateThisValue = "");
	}

	[DebuggerNonUserCode]
	private sealed class ParameterEqualsNotMatch<T> : TypedMatch<T>, IIsNotParameter<T>
	{
		private readonly T _value;
		private readonly string _valueExpression;
		private IEqualityComparer<T>? _comparer;
		private string? _comparerExpression;

		public ParameterEqualsNotMatch(T value, string valueExpression)
		{
			_value = value;
			_valueExpression = valueExpression;
		}

		/// <inheritdoc cref="IIsNotParameter{T}.Using(IEqualityComparer{T}, string)" />
		public IIsNotParameter<T> Using(IEqualityComparer<T> comparer, string doNotPopulateThisValue = "")
		{
			_comparer = comparer;
			_comparerExpression = doNotPopulateThisValue;
			return this;
		}

		/// <inheritdoc cref="TypedMatch{T}.Matches(T)" />
		protected override bool Matches(T value)
		{
			if (_comparer is not null)
			{
				return !_comparer.Equals(value, _value);
			}

			return !EqualityComparer<T>.Default.Equals(value, _value);
		}

		public override bool Matches(INamedParameterValue value)
		{
			if (value.TryGetValue(out T typedValue))
			{
				return Matches(typedValue);
			}

			return !value.IsNull || Matches(default(T)!);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
		{
			if (_comparer is not null)
			{
				return $"It.IsNot({_valueExpression}).Using({_comparerExpression})";
			}

			return $"It.IsNot({_valueExpression})";
		}
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
