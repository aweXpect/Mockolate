#if NET8_0_OR_GREATER
using System;
using Mockolate.Internals;
using Mockolate.Setup;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches any parameter of type <see cref="System.Span{T}"/> of <typeparamref name="T" />.
	/// </summary>
	public static IVerifyReadOnlySpanParameter<T> AnyReadOnlySpan<T>()
		=> new ReadOnlySpanParameterMatch<T>(null);

	/// <summary>
	///     Matches any parameter of type <see cref="System.Span{T}"/> of <typeparamref name="T" /> that matches the <paramref name="predicate"/>.
	/// </summary>
	public static IVerifyReadOnlySpanParameter<T> WithReadOnlySpan<T>(Func<T[], bool> predicate)
		=> new ReadOnlySpanParameterMatch<T>(predicate);

	private sealed class ReadOnlySpanParameterMatch<T> : TypedMatch<ReadOnlySpanWrapper<T>>, IVerifyReadOnlySpanParameter<T>
	{
		private Func<T[], bool>? predicate;

		public ReadOnlySpanParameterMatch(Func<T[], bool>? predicate)
		{
			this.predicate = predicate;
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"ReadOnlySpan<{typeof(T).FormatType()}>()";
		protected override bool Matches(ReadOnlySpanWrapper<T> value)
			=> predicate?.Invoke(value.ReadOnlySpanValues) ?? true;
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
#endif
