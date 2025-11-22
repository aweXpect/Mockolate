#if NET8_0_OR_GREATER
using System;

namespace Mockolate.Setup;

/// <summary>
///     Wraps a <see cref="ReadOnlySpan{T}" /> of <typeparamref name="T" /> to be used as a generic type parameter.
/// </summary>
public class ReadOnlySpanWrapper<T>
{
	/// <inheritdoc cref="ReadOnlySpanWrapper{T}" />
	public ReadOnlySpanWrapper(ReadOnlySpan<T> span)
	{
		ReadOnlySpanValues = span.ToArray();
	}

	/// <summary>
	///     Gets the array of values contained in the read-only span.
	/// </summary>
	public T[] ReadOnlySpanValues { get; }

	/// <summary>
	///     Implicitly converts a <see cref="ReadOnlySpanWrapper{T}" /> to a <see cref="ReadOnlySpan{T}" />.
	/// </summary>
	public static implicit operator ReadOnlySpan<T>(ReadOnlySpanWrapper<T> wrapper)
		=> new(wrapper.ReadOnlySpanValues);

	/// <summary>
	///     Implicitly converts a <see cref="ReadOnlySpan{T}" /> to a <see cref="ReadOnlySpanWrapper{T}" />.
	/// </summary>
	public static implicit operator ReadOnlySpanWrapper<T>(ReadOnlySpan<T> span)
		=> new(span);
}
#endif
