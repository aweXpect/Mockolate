using System;
using System.Runtime.CompilerServices;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches a parameter of type <typeparamref name="T" /> whose value makes <paramref name="predicate" /> return
	///     <see langword="true" />.
	/// </summary>
	/// <remarks>
	///     Use this when the built-in matchers aren't expressive enough (e.g. &quot;a <see cref="System.DateTime" /> in
	///     the past&quot; or &quot;a string that contains a substring&quot;). The predicate source expression is captured
	///     by the compiler and shown in failure messages, so keep lambdas short for readable diagnostics.
	/// </remarks>
	/// <typeparam name="T">The declared type of the parameter.</typeparam>
	/// <param name="predicate">The predicate that decides whether an argument matches.</param>
	/// <param name="doNotPopulateThisValue">Do not populate - captured automatically by the compiler.</param>
	/// <returns>A parameter matcher that delegates to <paramref name="predicate" />.</returns>
	/// <example>
	///     <code>
	///     sut.Mock.Setup.AddUser(It.Satisfies&lt;string&gt;(name =&gt; name.StartsWith("A")))
	///         .Returns(new User(id, "Alice"));
	///     </code>
	/// </example>
	public static IParameterWithCallback<T> Satisfies<T>(Func<T, bool> predicate,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue = "")
		=> new SatisfiesPredicateMatch<T>(predicate, doNotPopulateThisValue);

#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class SatisfiesPredicateMatch<T>(Func<T, bool> predicate, string predicateExpression) : TypedMatch<T>
	{
		protected override bool Matches(T value) => predicate(value);
		public override string ToString() => $"It.Satisfies<{typeof(T).FormatType()}>({predicateExpression})";
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
