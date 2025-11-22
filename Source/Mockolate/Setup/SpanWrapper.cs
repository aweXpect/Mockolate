#if NET8_0_OR_GREATER
using System;

namespace Mockolate.Setup;

/// <summary>
///     Wraps a <see cref="Span{T}" /> of <typeparamref name="T" /> to be used as a generic type parameter.
/// </summary>
public class SpanWrapper<T>
{
	/// <inheritdoc cref="SpanWrapper{T}" />
	public SpanWrapper(Span<T> span)
	{
		SpanValues = span.ToArray();
	}

	/// <summary>
	///     Gets the array of values contained in the span.
	/// </summary>
	public T[] SpanValues { get; }

	/// <summary>
	///     Implicitly converts a <see cref="SpanWrapper{T}" /> to a <see cref="Span{T}" />.
	/// </summary>
	public static implicit operator Span<T>(SpanWrapper<T> wrapper)
		=> new(wrapper.SpanValues);

	/// <summary>
	///     Implicitly converts a <see cref="Span{T}" /> to a <see cref="SpanWrapper{T}" />.
	/// </summary>
	public static implicit operator SpanWrapper<T>(Span<T> span)
		=> new(span);
}
#endif
