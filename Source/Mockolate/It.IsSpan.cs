#if NET8_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using Mockolate.Internals;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches any parameter of type <see cref="System.Span{T}" /> of <typeparamref name="T" />.
	/// </summary>
	public static IVerifySpanParameter<T> IsAnySpan<T>()
		=> new SpanParameterMatch<T>(null);

	/// <summary>
	///     Matches any parameter of type <see cref="System.Span{T}" /> of <typeparamref name="T" /> that matches the
	///     <paramref name="predicate" />.
	/// </summary>
	public static IVerifySpanParameter<T> IsSpan<T>(Func<T[], bool> predicate,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue = "")
		=> new SpanParameterMatch<T>(predicate, doNotPopulateThisValue);

	private sealed class SpanParameterMatch<T>(Func<T[], bool>? predicate, string? predicateExpression = null)
		: TypedMatch<SpanWrapper<T>>, IVerifySpanParameter<T>
	{
		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> predicate is null
				? $"It.IsAnySpan<{typeof(T).FormatType()}>()"
				: $"It.IsSpan<{typeof(T).FormatType()}>({predicateExpression})";

		protected override bool Matches(SpanWrapper<T> value)
			=> predicate?.Invoke(value.SpanValues) ?? true;
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
#endif
