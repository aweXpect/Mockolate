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
	///     Matches a parameter of type <typeparamref name="T" /> that satisfies the <paramref name="predicate" />.
	/// </summary>
	public static IParameter<T> Satisfies<T>(Func<T, bool> predicate,
		[CallerArgumentExpression("predicate")]
		string doNotPopulateThisValue = "")
		=> new SatisfiesPredicateMatch<T>(predicate, doNotPopulateThisValue);

	private sealed class SatisfiesPredicateMatch<T>(Func<T, bool> predicate, string predicateExpression) : TypedMatch<T>
	{
		protected override bool Matches(T value) => predicate(value);
		public override string ToString() => $"It.Satisfies<{typeof(T).FormatType()}>({predicateExpression})";
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
