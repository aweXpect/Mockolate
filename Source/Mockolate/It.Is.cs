using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches a parameter that equals <paramref name="value" />.
	/// </summary>
	/// <remarks>
	///     Equality uses <see cref="EqualityComparer{T}.Default" /> unless overridden via
	///     <see cref="IIsParameter{T}.Using" />. For methods and indexers with up to four parameters you can also pass
	///     the raw value directly &#8212; Mockolate treats it as if it were wrapped in <c>It.Is(value)</c>. Prefer an
	///     explicit <c>It.Is(...)</c> when the call site would otherwise be ambiguous (e.g. when mixing matchers and
	///     raw values), or when you need <see cref="IIsParameter{T}.Using"/> for a custom comparer.
	///     <para />
	///     The expression passed as <paramref name="value" /> is captured by the compiler (via
	///     <see cref="CallerArgumentExpressionAttribute" />) and appears verbatim in failure messages, so keep
	///     expressions short to get readable diagnostics.
	/// </remarks>
	/// <typeparam name="T">The declared type of the parameter.</typeparam>
	/// <param name="value">The expected value.</param>
	/// <param name="doNotPopulateThisValue">Do not populate - captured automatically by the compiler.</param>
	/// <returns>A parameter matcher that only accepts arguments equal to <paramref name="value" />.</returns>
	public static IIsParameter<T> Is<T>(T value,
		[CallerArgumentExpression(nameof(value))]
		string doNotPopulateThisValue = "")
		=> new ParameterEqualsMatch<T>(value, doNotPopulateThisValue);

	/// <summary>
	///     An <see cref="IParameter{T}" /> used for equality comparison, with an opt-in custom comparer.
	/// </summary>
	public interface IIsParameter<out T> : IParameterWithCallback<T>
	{
		/// <summary>
		///     Switches equality comparison to use <paramref name="comparer" /> instead of
		///     <see cref="EqualityComparer{T}.Default" />.
		/// </summary>
		/// <remarks>
		///     Useful for case-insensitive string comparison or structural comparisons on types that don't override
		///     <see cref="object.Equals(object)" />.
		/// </remarks>
		IIsParameter<T> Using(IEqualityComparer<T> comparer,
			[CallerArgumentExpression(nameof(comparer))]
			string doNotPopulateThisValue = "");
	}

#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class ParameterEqualsMatch<T>(T value, string valueExpression) : TypedMatch<T>, IIsParameter<T>
	{
		private IEqualityComparer<T>? _comparer;
		private string? _comparerExpression;

		/// <inheritdoc cref="IIsParameter{T}.Using(IEqualityComparer{T}, string)" />
		IIsParameter<T> IIsParameter<T>.Using(IEqualityComparer<T> comparer, string doNotPopulateThisValue)
		{
			_comparer = comparer;
			_comparerExpression = doNotPopulateThisValue;
			return this;
		}

		/// <inheritdoc cref="TypedMatch{T}.Matches(T)" />
		protected override bool Matches(T value1)
		{
			if (_comparer is not null)
			{
				return _comparer.Equals(value1, value);
			}

			return EqualityComparer<T>.Default.Equals(value1, value);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
		{
			if (_comparer is not null)
			{
				return $"It.Is({valueExpression}).Using({_comparerExpression})";
			}

			return valueExpression;
		}
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
