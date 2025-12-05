#if NET8_0_OR_GREATER
using System;
using Mockolate.Internals;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches any parameter of type <see cref="System.Span{T}" /> of <typeparamref name="T" />.
	/// </summary>
	public static IVerifySpanParameter<T> AnySpan<T>()
		=> new SpanParameterMatch<T>(null);

	/// <summary>
	///     Matches any parameter of type <see cref="System.Span{T}" /> of <typeparamref name="T" /> that matches the
	///     <paramref name="predicate" />.
	/// </summary>
	public static IVerifySpanParameter<T> WithSpan<T>(Func<T[], bool> predicate)
		=> new SpanParameterMatch<T>(predicate);

	private sealed class SpanParameterMatch<T>(Func<T[], bool>? predicate)
		: TypedMatch<SpanWrapper<T>>, IVerifySpanParameter<T>
	{
		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Span<{typeof(T).FormatType()}>()";

		protected override bool Matches(SpanWrapper<T> value)
			=> predicate?.Invoke(value.SpanValues) ?? true;
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
#endif
