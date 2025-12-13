using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches a parameter that is equal to one of the <paramref name="values" />.
	/// </summary>
	public static IIsOneOfParameter<T> IsOneOf<T>(params T[] values)
		=> new ParameterIsOneOfMatch<T>(values);

	/// <summary>
	///     An <see cref="IParameter{T}" /> used for equality comparison of a collection of alternatives.
	/// </summary>
	public interface IIsOneOfParameter<out T> : IParameter<T>
	{
		/// <summary>
		///     Use the specified comparer to determine equality.
		/// </summary>
		IIsOneOfParameter<T> Using(IEqualityComparer<T> comparer,
			[CallerArgumentExpression(nameof(comparer))]
			string doNotPopulateThisValue = "");
	}

	private sealed class ParameterIsOneOfMatch<T>(T[] values) : TypedMatch<T>, IIsOneOfParameter<T>
	{
		private IEqualityComparer<T>? _comparer;
		private string? _comparerExpression;

		/// <inheritdoc cref="IIsParameter{T}.Using(IEqualityComparer{T}, string)" />
		public IIsOneOfParameter<T> Using(IEqualityComparer<T> comparer, string doNotPopulateThisValue = "")
		{
			_comparer = comparer;
			_comparerExpression = doNotPopulateThisValue;
			return this;
		}

		/// <inheritdoc cref="TypedMatch{T}.Matches(T)" />
		protected override bool Matches(T value)
		{
			IEqualityComparer<T> comparer = _comparer ?? EqualityComparer<T>.Default;
			return values.Any(v => comparer.Equals(value, v));
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
		{
			string result =
				$"It.IsOneOf({string.Join(", ", values.Select(v => v is string ? $"\"{v}\"" : v?.ToString() ?? "null"))})";
			if (_comparer is not null)
			{
				result += $".Using({_comparerExpression})";
			}

			return result;
		}
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
