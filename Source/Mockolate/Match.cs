using System;
using System.Collections.Generic;
#if NET8_0_OR_GREATER
using Mockolate.Setup;

// ReSharper disable UnusedTypeParameter
#endif

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

	private static bool TryCast<T>(object? value, out T typedValue)
	{
		if (value is T castValue)
		{
			typedValue = castValue;
			return true;
		}

		if (value is null)
		{
			typedValue = default!;
			return true;
		}

		typedValue = default!;
		return false;
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

		/// <summary>
		///     Invokes the callbacks registered for this parameter match.
		/// </summary>
		void InvokeCallbacks(object? value);
	}

#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	public interface IParameter<out T>
	{
		/// <summary>
		///     Registers a <paramref name="callback" /> to execute for matching parameters.
		/// </summary>
		IParameter<T> Do(Action<T> callback);
	}
#pragma warning restore S2326 // Unused type parameters should be removed

#if NET8_0_OR_GREATER
#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches a method parameter of type <see cref="System.Span{T}" /> of <typeparamref name="T" /> against an
	///     expectation.
	/// </summary>
	public interface ISpanParameter<T> : IParameter<SpanWrapper<T>>
	{
	}
#pragma warning restore S2326 // Unused type parameters should be removed
#endif

#if NET8_0_OR_GREATER
#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches a method parameter of type <see cref="System.Span{T}" /> of <typeparamref name="T" /> against an
	///     expectation.
	/// </summary>
	public interface IVerifySpanParameter<T> : ISpanParameter<T>
	{
	}
#pragma warning restore S2326 // Unused type parameters should be removed
#endif

#if NET8_0_OR_GREATER
#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches a method parameter of type <see cref="System.ReadOnlySpan{T}" /> of <typeparamref name="T" /> against an
	///     expectation.
	/// </summary>
	public interface IReadOnlySpanParameter<T> : IParameter<ReadOnlySpanWrapper<T>>
	{
	}
#pragma warning restore S2326 // Unused type parameters should be removed
#endif

#if NET8_0_OR_GREATER
#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches a method parameter of type <see cref="System.ReadOnlySpan{T}" /> of <typeparamref name="T" /> against an
	///     expectation.
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
		bool Matches(object?[] values);
	}

	/// <summary>
	///     Matches an <see langword="out" /> parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	public interface IOutParameter<out T>
	{
		/// <summary>
		///     Retrieves the value to which the <see langword="out" /> parameter should be set.
		/// </summary>
		T GetValue(MockBehavior mockBehavior);

		/// <summary>
		///     Registers a <paramref name="callback" /> to execute for matching parameters.
		/// </summary>
		IOutParameter<T> Do(Action<T> callback);
	}

#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches any <see langword="out" /> parameter.
	/// </summary>
	public interface IVerifyOutParameter<out T>
	{
	}
#pragma warning restore S2326 // Unused type parameters should be removed

	/// <summary>
	///     Matches an <see langword="ref" /> parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	public interface IRefParameter<T>
	{
		/// <summary>
		///     Retrieves the value to which the <see langword="out" /> parameter should be set.
		/// </summary>
		T GetValue(T value);

		/// <summary>
		///     Registers a <paramref name="callback" /> to execute for matching parameters.
		/// </summary>
		IRefParameter<T> Do(Action<T> callback);
	}

#pragma warning disable S2326 // Unused type parameters should be removed
	/// <summary>
	///     Matches any <see langword="ref" /> parameter.
	/// </summary>
	public interface IVerifyRefParameter<T>
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
	///     Provides monitoring capabilities for parameters of the specified type from a mocked method or indexer,
	///     allowing inspection of actual matched values.
	/// </summary>
	public interface IParameterMonitor<out T>
	{
		/// <summary>
		///     Verifies the interactions with the mocked subject of <typeparamref name="T" />.
		/// </summary>
		IReadOnlyList<T> Values { get; }
	}

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	private abstract class TypedMatch<T> : IParameter<T>, IParameter
	{
		private List<Action<T>>? _callbacks;

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

		public void InvokeCallbacks(object? value)
		{
			if (TryCast(value, out T typedValue) && _callbacks is not null)
			{
				_callbacks.ForEach(a => a.Invoke(typedValue));
			}
		}

		public IParameter<T> Do(Action<T> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		/// <summary>
		///     Verifies the expectation for the <paramref name="value" />.
		/// </summary>
		protected abstract bool Matches(T value);
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
