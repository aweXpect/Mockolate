using System;
using System.Runtime.CompilerServices;
using Mockolate.Internals;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Matches any <see langword="out" /> parameter of type <typeparamref name="T" />.
	/// </summary>
	public static IVerifyOutParameter<T> Out<T>()
		=> new InvokedOutParameterMatch<T>();

	/// <summary>
	///     Matches any <see langword="out" /> parameter of type <typeparamref name="T" /> and
	///     uses the <paramref name="setter" /> to set the value when the method is invoked.
	/// </summary>
	public static IOutParameter<T> Out<T>(Func<T> setter,
		[CallerArgumentExpression("setter")] string doNotPopulateThisValue = "")
		=> new OutParameterMatch<T>(setter, doNotPopulateThisValue);

	/// <summary>
	///     Matches an <see langword="out" /> parameter against an expectation.
	/// </summary>
	private sealed class OutParameterMatch<T>(Func<T> setter, string setterExpression) : IOutParameter<T>
	{
		/// <inheritdoc cref="IParameter.Matches(object?)" />
		public bool Matches(object? value) => true;

		/// <inheritdoc cref="IOutParameter{T}.GetValue(MockBehavior)" />
		public T GetValue(MockBehavior mockBehavior) => setter();

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Out<{typeof(T).FormatType()}>({setterExpression})";
	}

	/// <summary>
	///     Matches any <see langword="out" /> parameter.
	/// </summary>
	private sealed class InvokedOutParameterMatch<T> : IVerifyOutParameter<T>
	{
		/// <inheritdoc cref="IParameter.Matches(object?)" />
		public bool Matches(object? value) => true;

		/// <inheritdoc cref="IOutParameter{T}.GetValue(MockBehavior)" />
		public T GetValue(MockBehavior mockBehavior)
			=> mockBehavior.DefaultValue.Generate<T>();

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => $"Out<{typeof(T).FormatType()}>()";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
