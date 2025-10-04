using System.Collections;

namespace Mockolate.SourceGenerators.Internals;

/// <summary>
///     An immutable, equatable array. This is equivalent to <see cref="Array" /> but with value equality support.
/// </summary>
/// <typeparam name="T">The type of values in the array.</typeparam>
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
	where T : IEquatable<T>
{
	private readonly T[]? _array;

	/// <see cref="EquatableArray{T}" />
	public EquatableArray(T[] array)
	{
		_array = array;
	}

	/// <summary>
	///     Gets the length of the array, or 0 if the array is null
	/// </summary>
	public int Count => _array?.Length ?? 0;

	/// <summary>
	///     Checks whether two <see cref="EquatableArray{T}" /> values are the same.
	/// </summary>
	/// <param name="left">The first <see cref="EquatableArray{T}" /> value.</param>
	/// <param name="right">The second <see cref="EquatableArray{T}" /> value.</param>
	/// <returns>Whether <paramref name="left" /> and <paramref name="right" /> are equal.</returns>
	public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
	{
		return left.Equals(right);
	}

	/// <summary>
	///     Checks whether two <see cref="EquatableArray{T}" /> values are not the same.
	/// </summary>
	/// <param name="left">The first <see cref="EquatableArray{T}" /> value.</param>
	/// <param name="right">The second <see cref="EquatableArray{T}" /> value.</param>
	/// <returns>Whether <paramref name="left" /> and <paramref name="right" /> are not equal.</returns>
	public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
	{
		return !left.Equals(right);
	}

	/// <inheritdoc />
	public bool Equals(EquatableArray<T> array) => AsSpan().SequenceEqual(array.AsSpan());

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is EquatableArray<T> array && Equals(array);

	/// <inheritdoc />
	public override int GetHashCode()
	{
		if (_array is not T[] array)
		{
			return 0;
		}

		int hashCode = 0;
		int multiplier = 1;

		foreach (T item in array)
		{
			hashCode += item.GetHashCode() * multiplier;
			multiplier *= 17;
		}

		return hashCode;
	}

	/// <summary>
	///     Returns a <see cref="ReadOnlySpan{T}" /> wrapping the current items.
	/// </summary>
	/// <returns>A <see cref="ReadOnlySpan{T}" /> wrapping the current items.</returns>
	public ReadOnlySpan<T> AsSpan() => _array.AsSpan();

	/// <summary>
	///     Returns the underlying wrapped array.
	/// </summary>
	/// <returns>Returns the underlying array.</returns>
	public T[]? AsArray() => _array;

	/// <inheritdoc />
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)(_array ?? Array.Empty<T>())).GetEnumerator();

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)(_array ?? Array.Empty<T>())).GetEnumerator();
}
