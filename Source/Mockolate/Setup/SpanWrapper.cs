#if NET8_0_OR_GREATER
using System;

namespace Mockolate.Setup;

/// <summary>
///     Wraps a <see cref="Span{T}"/> of <typeparamref name="T"/> to be used as a generic type parameter.
/// </summary>
public class SpanWrapper<T>
{
	/// <summary>
	///     Gets the array of values contained in the span.
	/// </summary>
	public T[] SpanValues { get; }

	/// <inheritdoc cref="SpanWrapper{T}" />
	public SpanWrapper(Span<T> span)
	{
		SpanValues = span.ToArray();
	}
}
#endif
