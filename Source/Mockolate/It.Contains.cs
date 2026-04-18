using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches a collection parameter that contains <paramref name="item" />.
	/// </summary>
	/// <remarks>
	///     Supports method parameters declared as <see cref="IEnumerable{T}" />, <see cref="ICollection{T}" />,
	///     <see cref="IList{T}" />, <see cref="IReadOnlyCollection{T}" />, <see cref="IReadOnlyList{T}" />,
	///     <see cref="ISet{T}" />, <typeparamref name="T" /> arrays, <see cref="List{T}" />,
	///     <see cref="HashSet{T}" />, <see cref="Queue{T}" /> or <see cref="Stack{T}" />.
	/// </remarks>
	public static IContainsParameter<T> Contains<T>(T item,
		[CallerArgumentExpression(nameof(item))]
		string doNotPopulateThisValue = "")
		=> new ParameterContainsMatch<T>(item, doNotPopulateThisValue);

	/// <summary>
	///     An <see cref="IParameter{T}" /> that matches any supported collection parameter that contains an item.
	/// </summary>
	public interface IContainsParameter<T> :
		IParameter<IEnumerable<T>>,
		IParameter<ICollection<T>>,
		IParameter<IList<T>>,
		IParameter<IReadOnlyCollection<T>>,
		IParameter<IReadOnlyList<T>>,
		IParameter<ISet<T>>,
		IParameter<T[]>,
		IParameter<List<T>>,
		IParameter<HashSet<T>>,
		IParameter<Queue<T>>,
		IParameter<Stack<T>>
	{
		/// <summary>
		///     Use the specified comparer to determine equality for the contained item.
		/// </summary>
		IContainsParameter<T> Using(IEqualityComparer<T> comparer,
			[CallerArgumentExpression(nameof(comparer))]
			string doNotPopulateThisValue = "");
	}

#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class ParameterContainsMatch<T>(T item, string itemExpression)
		: CollectionMatch<T>, IContainsParameter<T>
	{
		private IEqualityComparer<T>? _comparer;
		private string? _comparerExpression;

		/// <inheritdoc cref="IContainsParameter{T}.Using(IEqualityComparer{T}, string)" />
		IContainsParameter<T> IContainsParameter<T>.Using(IEqualityComparer<T> comparer,
			string doNotPopulateThisValue)
		{
			_comparer = comparer;
			_comparerExpression = doNotPopulateThisValue;
			return this;
		}

		/// <inheritdoc cref="CollectionMatchCore{T}.MatchesCollection(IEnumerable{T})" />
		protected override bool MatchesCollection(IEnumerable<T> value)
		{
			IEqualityComparer<T> comparer = _comparer ?? EqualityComparer<T>.Default;
			return value.Contains(item, comparer);
		}

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString()
		{
			string result = $"It.Contains({itemExpression})";
			if (_comparer is not null) result += $".Using({_comparerExpression})";

			return result;
		}
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
