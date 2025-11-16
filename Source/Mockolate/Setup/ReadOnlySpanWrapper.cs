#if NET8_0_OR_GREATER
using System;

namespace Mockolate.Setup;

/// <summary>
///     Wraps a <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> to be used as a generic type parameter.
/// </summary>
public class ReadOnlySpanWrapper<T>
{
	/// <summary>
	///     Gets the array of values contained in the read-only span.
	/// </summary>
	public T[] ReadOnlySpanValues { get; }

	/// <inheritdoc cref="SpanWrapper{T}" />
	public ReadOnlySpanWrapper(ReadOnlySpan<T> span)
	{
		ReadOnlySpanValues = span.ToArray();
	}
}
#endif
