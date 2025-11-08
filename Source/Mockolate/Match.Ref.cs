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
		=> new RefParameter<T>(_ => true, setter, null, doNotPopulateThisValue);

	/// <summary>
	///     Matches a <see langword="ref" /> parameter of type <typeparamref name="T" /> that satisfies the
	///     <paramref name="predicate" /> and
	///     uses the <paramref name="setter" /> to set the value when the method is invoked.
	/// </summary>
	public static IRefParameter<T> Ref<T>(Func<T, bool> predicate, Func<T, T> setter,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue1 = "",
		[CallerArgumentExpression("setter")] string doNotPopulateThisValue2 = "")
		=> new RefParameter<T>(predicate, setter, doNotPopulateThisValue1, doNotPopulateThisValue2);

	/// <summary>
	///     Matches any <see langword="ref" /> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static IVerifyRefParameter<T> Ref<T>()
		=> new InvokedRefParameter<T>();

	/// <summary>
	///     Matches a method <see langword="ref" /> parameter against an expectation.
	/// </summary>
	private sealed class RefParameter<T>(
		Func<T, bool> predicate,
		Func<T, T> setter,
		string? predicateExpression,
		string setterExpression) : IRefParameter<T>
	{
		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter
		///     of type <typeparamref name="T" />; otherwise <see langword="false" />.
		/// </returns>
		public bool Matches(object? value) => value is T typedValue && predicate(typedValue);

		/// <summary>
		///     Retrieves the value to which the <see langword="ref" /> parameter should be set.
		/// </summary>
		public T GetValue(T value) => setter(value);

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
			=> $"Ref<{typeof(T).FormatType()}>({(predicateExpression is null ? "" : $"{predicateExpression}, ")}{setterExpression})";
	}

	/// <summary>
	///     Matches a method <see langword="out" /> parameter against an expectation.
	/// </summary>
	private sealed class InvokedRefParameter<T> : IVerifyRefParameter<T>
	{
		/// <summary>
		///     Matches any <paramref name="value" />.
		/// </summary>
		public bool Matches(object? value) => true;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Ref<{typeof(T).FormatType()}>()";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
