using System;
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
	///     Matches a method <see langword="ref" /> parameter against an expectation.
	/// </summary>
	private sealed class RefParameterMatch<T>(
		Func<T, bool> predicate,
		Func<T, T>? setter,
		string? predicateExpression,
		string? setterExpression) : IRefParameter<T>
	{
		/// <inheritdoc cref="IParameter.Matches(object?)" />
		public bool Matches(object? value) => value is T typedValue && predicate(typedValue);

		/// <inheritdoc cref="IRefParameter{T}.GetValue(T)" />
		public T GetValue(T value)
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
	///     Matches a method <see langword="out" /> parameter against an expectation.
	/// </summary>
	private sealed class InvokedRefParameterMatch<T> : IVerifyRefParameter<T>
	{
		/// <inheritdoc cref="IParameter.Matches(object?)" />
		public bool Matches(object? value) => true;

		/// <inheritdoc cref="IRefParameter{T}.GetValue(T)" />
		public T GetValue(T value) => value;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Ref<{typeof(T).FormatType()}>()";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
