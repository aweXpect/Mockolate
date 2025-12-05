#if NET8_0_OR_GREATER
using System;
using Mockolate.Internals;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class It
{
	/// <summary>
	///     Matches any parameter of type <see cref="System.ReadOnlySpan{T}" /> of <typeparamref name="T" />.
	/// </summary>
	public static IVerifyReadOnlySpanParameter<T> IsAnyReadOnlySpan<T>()
		=> new ReadOnlySpanParameterMatch<T>(null);

	/// <summary>
	///     Matches any parameter of type <see cref="System.ReadOnlySpan{T}" /> of <typeparamref name="T" /> that matches the
	///     <paramref name="predicate" />.
	/// </summary>
	public static IVerifyReadOnlySpanParameter<T> IsReadOnlySpan<T>(Func<T[], bool> predicate)
		=> new ReadOnlySpanParameterMatch<T>(predicate);

	private sealed class ReadOnlySpanParameterMatch<T>(Func<T[], bool>? predicate)
		: TypedMatch<ReadOnlySpanWrapper<T>>, IVerifyReadOnlySpanParameter<T>
	{
		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"It.IsReadOnlySpan<{typeof(T).FormatType()}>()";

		protected override bool Matches(ReadOnlySpanWrapper<T> value)
			=> predicate?.Invoke(value.ReadOnlySpanValues) ?? true;
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
#endif
