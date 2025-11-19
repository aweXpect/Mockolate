using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mockolate.Internals;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches any <see langword="ref" /> parameter of type <typeparamref name="T" /> and
	///     uses the <paramref name="setter" /> to set the value when the method is invoked.
	/// </summary>
	public static IRefParameter<T> Ref<T>(Func<T, T> setter,
		[CallerArgumentExpression("setter")] string doNotPopulateThisValue = "")
		=> new RefParameterMatch<T>(_ => true, setter, null, doNotPopulateThisValue);

	/// <summary>
	///     Matches a <see langword="ref" /> parameter of type <typeparamref name="T" /> that satisfies the
	///     <paramref name="predicate" /> and
	///     uses the <paramref name="setter" /> to set the value when the method is invoked.
	/// </summary>
	public static IRefParameter<T> Ref<T>(Func<T, bool> predicate, Func<T, T> setter,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue1 = "",
		[CallerArgumentExpression("setter")] string doNotPopulateThisValue2 = "")
		=> new RefParameterMatch<T>(predicate, setter, doNotPopulateThisValue1, doNotPopulateThisValue2);

	/// <summary>
	///     Matches a <see langword="ref" /> parameter of type <typeparamref name="T" /> that satisfies the
	///     <paramref name="predicate" />.
	/// </summary>
	public static IRefParameter<T> Ref<T>(Func<T, bool> predicate,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue = "")
		=> new RefParameterMatch<T>(predicate, null, doNotPopulateThisValue, null);

	/// <summary>
	///     Matches any <see langword="ref" /> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static IVerifyRefParameter<T> Ref<T>()
		=> new InvokedRefParameterMatch<T>();

	/// <summary>
	///     Matches any <see langword="ref" /> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static IRefParameter<T> AnyRef<T>()
		=> new AnyRefParameterMatch<T>();

	/// <summary>
	///     Matches a method <see langword="ref" /> parameter against an expectation.
	/// </summary>
	private sealed class RefParameterMatch<T>(
		Func<T, bool> predicate,
		Func<T, T>? setter,
		string? predicateExpression,
		string? setterExpression) : TypedRefMatch<T>
	{
		/// <inheritdoc cref="TypedRefMatch{T}.Matches(T)" />
		protected override bool Matches(T value)
			=> predicate(value);

		/// <inheritdoc cref="IRefParameter{T}.GetValue(T)" />
		public override T GetValue(T value)
		{
			if (setter is null)
			{
				return value;
			}

			return setter(value);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> (predicateExpression is not null, setterExpression is not null) switch
			{
				(true, true) => $"Ref<{typeof(T).FormatType()}>({predicateExpression}, {setterExpression})",
				(true, false) => $"Ref<{typeof(T).FormatType()}>({predicateExpression})",
				(false, true) => $"Ref<{typeof(T).FormatType()}>({setterExpression})",
				(false, false) => $"Ref<{typeof(T).FormatType()}>()",
			};
	}

	/// <summary>
	///     Matches any method <see langword="ref" /> parameter.
	/// </summary>
	private sealed class AnyRefParameterMatch<T> : TypedRefMatch<T>
	{
		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"AnyRef<{typeof(T).FormatType()}>()";

		/// <inheritdoc cref="TypedRefMatch{T}.Matches(T)" />
		protected override bool Matches(T value)
			=> true;
	}

	/// <summary>
	///     Matches a method <see langword="out" /> parameter against an expectation.
	/// </summary>
	private sealed class InvokedRefParameterMatch<T> : IVerifyRefParameter<T>, IParameter
	{
		/// <inheritdoc cref="IParameter.Matches(object?)" />
		public bool Matches(object? value) => true;

		/// <inheritdoc cref="IParameter.InvokeCallbacks(object?)" />
		public void InvokeCallbacks(object? value)
		{
			// Do nothing
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Ref<{typeof(T).FormatType()}>()";
	}

	/// <summary>
	///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
	/// </summary>
	private abstract class TypedRefMatch<T> : IRefParameter<T>, IParameter
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

		/// <inheritdoc cref="IParameter.InvokeCallbacks(object?)" />
		public void InvokeCallbacks(object? value)
		{
			if (TryCast(value, out T typedValue) && _callbacks is not null)
			{
				_callbacks.ForEach(a => a.Invoke(typedValue));
			}
		}

		/// <inheritdoc cref="IRefParameter{T}.GetValue(T)" />
		public virtual T GetValue(T value)
			=> value;

		/// <inheritdoc cref="IRefParameter{T}.Do(Action{T})" />
		public IRefParameter<T> Do(Action<T> callback)
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
