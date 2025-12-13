using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches a parameter that is equal to <paramref name="value" />.
	/// </summary>
	public static IIsParameter<T> Is<T>(T value,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue = "")
		=> new ParameterEqualsMatch<T>(value, doNotPopulateThisValue);

	/// <summary>
	///     An <see cref="IParameter{T}" /> used for equality comparison.
	/// </summary>
	public interface IIsParameter<out T> : IParameter<T>
	{
		/// <summary>
		///     Use the specified comparer to determine equality.
		/// </summary>
		IIsParameter<T> Using(IEqualityComparer<T> comparer,
			[CallerArgumentExpression(nameof(comparer))]
			string doNotPopulateThisValue = "");
	}

	private sealed class ParameterEqualsMatch<T> : TypedMatch<T>, IIsParameter<T>
	{
		private readonly T _value;
		private readonly string _valueExpression;
		private IEqualityComparer<T>? _comparer;
		private string? _comparerExpression;

		public ParameterEqualsMatch(T value, string valueExpression)
		{
			_value = value;
			_valueExpression = valueExpression;
		}

		/// <inheritdoc cref="IIsParameter{T}.Using(IEqualityComparer{T}, string)" />
		public IIsParameter<T> Using(IEqualityComparer<T> comparer, string doNotPopulateThisValue = "")
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
				return _comparer.Equals(value, _value);
			}

			return EqualityComparer<T>.Default.Equals(value, _value);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
		{
			if (_comparer is not null)
			{
				return $"It.Is({_valueExpression}).Using({_comparerExpression})";
			}

			return _valueExpression;
		}
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
