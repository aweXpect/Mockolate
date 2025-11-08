using System;
using System.Runtime.CompilerServices;
using Mockolate.Internals;
using Mockolate.Match;

namespace Mockolate;

public partial class Parameter
{
	/// <summary>
	///     Matches any <see langword="out" /> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static IVerifyOutParameter<T> Out<T>()
		=> new InvokedOutParameter<T>();

	/// <summary>
	///     Matches any <see langword="out" /> parameter of type <typeparamref name="T" /> and
	///     uses the <paramref name="setter" /> to set the value when the method is invoked.
	/// </summary>
	public static IOutParameter<T> Out<T>(Func<T> setter,
		[CallerArgumentExpression("setter")] string doNotPopulateThisValue = "")
		=> new OutParameter<T>(setter, doNotPopulateThisValue);

	/// <summary>
	///     Matches an <see langword="out" /> parameter against an expectation.
	/// </summary>
	private sealed class OutParameter<T>(Func<T> setter, string setterExpression) : IOutParameter<T>
	{
		/// <summary>
		///     Checks if the <paramref name="value" /> is a matching parameter.
		/// </summary>
		/// <returns>
		///     <see langword="true" />, if the <paramref name="value" /> is a matching parameter
		///     of type <typeparamref name="T" />; otherwise <see langword="false" />.
		/// </returns>
		public bool Matches(object? value) => true;

		/// <summary>
		///     Retrieves the value to which the <see langword="out" /> parameter should be set.
		/// </summary>
		public T GetValue() => setter();

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Out<{typeof(T).FormatType()}>({setterExpression})";
	}

	/// <summary>
	///     Matches any <see langword="out" /> parameter.
	/// </summary>
	private sealed class InvokedOutParameter<T> : IVerifyOutParameter<T>
	{
		/// <summary>
		///     Matches any <paramref name="value" />.
		/// </summary>
		public bool Matches(object? value) => true;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Out<{typeof(T).FormatType()}>()";
	}
}
