using System;
using Mockolate.Setup;
using static Mockolate.Match;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
/// <summary>
///     Specify a matching condition for a method parameter.
/// </summary>
public partial class Match
{
	private Match()
	{
		// Prevent instantiation.
	}

	/// <summary>
	///     Matches a method parameter against an expectation.
	/// </summary>
	public interface IParameter
	{
		/// <summary>
		///     Checks if the <paramref name="value" /> matches the expectation.
		/// </summary>
		bool Matches(object? value);
	}

#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	public interface IParameter<out T> : IParameter
	{
	}
#pragma warning restore S2326 // Unused type parameters should be removed

#if NET8_0_OR_GREATER
#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches a method parameter of type <see cref="System.Span{T}"/> of <typeparamref name="T" /> against an expectation.
	/// </summary>
	public interface ISpanParameter<T> : IParameter<SpanWrapper<T>>
	{
	}
#pragma warning restore S2326 // Unused type parameters should be removed
#endif

#if NET8_0_OR_GREATER
#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches a method parameter of type <see cref="System.Span{T}"/> of <typeparamref name="T" /> against an expectation.
	/// </summary>
	public interface IVerifySpanParameter<T> : ISpanParameter<T>
	{
	}
#pragma warning restore S2326 // Unused type parameters should be removed
#endif

#if NET8_0_OR_GREATER
#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches a method parameter of type <see cref="System.ReadOnlySpan{T}"/> of <typeparamref name="T" /> against an expectation.
	/// </summary>
	public interface IReadOnlySpanParameter<T> : IParameter<ReadOnlySpanWrapper<T>>
	{
	}
#pragma warning restore S2326 // Unused type parameters should be removed
#endif

#if NET8_0_OR_GREATER
#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches a method parameter of type <see cref="System.ReadOnlySpan{T}"/> of <typeparamref name="T" /> against an expectation.
	/// </summary>
	public interface IVerifyReadOnlySpanParameter<T> : IReadOnlySpanParameter<T>
	{
	}
#pragma warning restore S2326 // Unused type parameters should be removed
#endif

	/// <summary>
	///     Matches the method parameters against an expectation.
	/// </summary>
	public interface IParameters
	{
		/// <summary>
		///     Checks if the <paramref name="values" /> match the expectations.
		/// </summary>
		abstract bool Matches(object?[] values);
	}

	/// <summary>
	///     Matches an <see langword="out" /> parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	public interface IOutParameter<out T> : IParameter
	{
		/// <summary>
		///     Retrieves the value to which the <see langword="out" /> parameter should be set.
		/// </summary>
		T GetValue(MockBehavior mockBehavior);
	}

#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches any <see langword="out" /> parameter.
	/// </summary>
	public interface IVerifyOutParameter<out T> : IOutParameter<T>
	{
	}
#pragma warning restore S2326 // Unused type parameters should be removed

	/// <summary>
	///     Matches an <see langword="ref" /> parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	public interface IRefParameter<T> : IParameter
	{
		/// <summary>
		///     Retrieves the value to which the <see langword="out" /> parameter should be set.
		/// </summary>
		T GetValue(T value);
	}

#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches any <see langword="ref" /> parameter.
	/// </summary>
	public interface IVerifyRefParameter<T> : IRefParameter<T>
	{
	}
#pragma warning restore S2326 // Unused type parameters should be removed

	/// <summary>
	///     Use default event parameters when raising events.
	/// </summary>
	public interface IDefaultEventParameters
	{
	}

	/// <summary>
	///     A named <see cref="Parameter" />.
	/// </summary>
	/// <param name="Name">The name of the <paramref name="Parameter" />.</param>
	/// <param name="Parameter">The actual <see cref="IParameter" />.</param>
	public record NamedParameter(string Name, IParameter Parameter)
	{
		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"{Parameter} {Name}";
	}

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	private abstract class TypedMatch<T> : IParameter<T>
	{
		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter
		///     of type <typeparamref name="T" />; otherwise <see langword="false" />.
		/// </returns>
		public bool Matches(object? value)
		{
			if (value is T typedValue)
			{
				return Matches(typedValue);
			}

			return value is null && Matches(default!);
		}

		/// <summary>
		///     Verifies the expectation for the <paramref name="value" />.
		/// </summary>
		protected abstract bool Matches(T value);
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
