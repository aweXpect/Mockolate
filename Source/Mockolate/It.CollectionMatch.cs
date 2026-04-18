using System;
using System.Collections.Generic;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Base for matchers that target any BCL collection of <typeparamref name="T" />.
	///     Implements <see cref="IParameter{TCollection}" /> and <see cref="IParameterMatch{TCollection}" /> for all
	///     common collection shapes so the matcher fits setup slots typed as arrays, <see cref="List{T}" />,
	///     <see cref="HashSet{T}" />, <see cref="Queue{T}" />, <see cref="Stack{T}" />, or any of the corresponding
	///     collection interfaces. Actual matching runs through the non-generic
	///     <see cref="IParameter.Matches(object?)" /> path, which dispatches to <see cref="MatchesCollection" /> once
	///     the incoming value is confirmed to be an <see cref="IEnumerable{T}" />.
	/// </summary>
#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private abstract class CollectionMatch<T> :
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
		IParameter<Stack<T>>,
		IParameterMatch<IEnumerable<T>>,
		IParameterMatch<ICollection<T>>,
		IParameterMatch<IList<T>>,
		IParameterMatch<IReadOnlyCollection<T>>,
		IParameterMatch<IReadOnlyList<T>>,
		IParameterMatch<ISet<T>>,
		IParameterMatch<T[]>,
		IParameterMatch<List<T>>,
		IParameterMatch<HashSet<T>>,
		IParameterMatch<Queue<T>>,
		IParameterMatch<Stack<T>>
	{
		private List<Action<object?>>? _callbacks;

		/// <summary>
		///     Performs the actual collection-level match.
		/// </summary>
		protected abstract bool MatchesCollection(IEnumerable<T> value);

		bool IParameter.Matches(object? value)
			=> value is IEnumerable<T> typed && MatchesCollection(typed);

		void IParameter.InvokeCallbacks(object? value) => InvokeAll(value);

		private void InvokeAll(object? value)
		{
			if (_callbacks is null || value is not IEnumerable<T>) return;

			_callbacks.ForEach(c => c.Invoke(value));
		}

		private void Register<TCollection>(Action<TCollection> callback)
			where TCollection : class, IEnumerable<T>
		{
			_callbacks ??= [];
			_callbacks.Add(obj =>
			{
				if (obj is TCollection typed) callback(typed);
			});
		}

		IParameter<IEnumerable<T>> IParameter<IEnumerable<T>>.Do(Action<IEnumerable<T>> callback)
		{
			Register(callback);
			return this;
		}

		IParameter<ICollection<T>> IParameter<ICollection<T>>.Do(Action<ICollection<T>> callback)
		{
			Register(callback);
			return this;
		}

		IParameter<IList<T>> IParameter<IList<T>>.Do(Action<IList<T>> callback)
		{
			Register(callback);
			return this;
		}

		IParameter<IReadOnlyCollection<T>> IParameter<IReadOnlyCollection<T>>.Do(Action<IReadOnlyCollection<T>> callback)
		{
			Register(callback);
			return this;
		}

		IParameter<IReadOnlyList<T>> IParameter<IReadOnlyList<T>>.Do(Action<IReadOnlyList<T>> callback)
		{
			Register(callback);
			return this;
		}

		IParameter<ISet<T>> IParameter<ISet<T>>.Do(Action<ISet<T>> callback)
		{
			Register(callback);
			return this;
		}

		IParameter<T[]> IParameter<T[]>.Do(Action<T[]> callback)
		{
			Register(callback);
			return this;
		}

		IParameter<List<T>> IParameter<List<T>>.Do(Action<List<T>> callback)
		{
			Register(callback);
			return this;
		}

		IParameter<HashSet<T>> IParameter<HashSet<T>>.Do(Action<HashSet<T>> callback)
		{
			Register(callback);
			return this;
		}

		IParameter<Queue<T>> IParameter<Queue<T>>.Do(Action<Queue<T>> callback)
		{
			Register(callback);
			return this;
		}

		IParameter<Stack<T>> IParameter<Stack<T>>.Do(Action<Stack<T>> callback)
		{
			Register(callback);
			return this;
		}

		bool IParameterMatch<IEnumerable<T>>.Matches(IEnumerable<T> value) => MatchesCollection(value);
		bool IParameterMatch<ICollection<T>>.Matches(ICollection<T> value) => MatchesCollection(value);
		bool IParameterMatch<IList<T>>.Matches(IList<T> value) => MatchesCollection(value);
		bool IParameterMatch<IReadOnlyCollection<T>>.Matches(IReadOnlyCollection<T> value) => MatchesCollection(value);
		bool IParameterMatch<IReadOnlyList<T>>.Matches(IReadOnlyList<T> value) => MatchesCollection(value);
		bool IParameterMatch<ISet<T>>.Matches(ISet<T> value) => MatchesCollection(value);
		bool IParameterMatch<T[]>.Matches(T[] value) => MatchesCollection(value);
		bool IParameterMatch<List<T>>.Matches(List<T> value) => MatchesCollection(value);
		bool IParameterMatch<HashSet<T>>.Matches(HashSet<T> value) => MatchesCollection(value);
		bool IParameterMatch<Queue<T>>.Matches(Queue<T> value) => MatchesCollection(value);
		bool IParameterMatch<Stack<T>>.Matches(Stack<T> value) => MatchesCollection(value);

		void IParameterMatch<IEnumerable<T>>.InvokeCallbacks(IEnumerable<T> value) => InvokeAll(value);
		void IParameterMatch<ICollection<T>>.InvokeCallbacks(ICollection<T> value) => InvokeAll(value);
		void IParameterMatch<IList<T>>.InvokeCallbacks(IList<T> value) => InvokeAll(value);
		void IParameterMatch<IReadOnlyCollection<T>>.InvokeCallbacks(IReadOnlyCollection<T> value) => InvokeAll(value);
		void IParameterMatch<IReadOnlyList<T>>.InvokeCallbacks(IReadOnlyList<T> value) => InvokeAll(value);
		void IParameterMatch<ISet<T>>.InvokeCallbacks(ISet<T> value) => InvokeAll(value);
		void IParameterMatch<T[]>.InvokeCallbacks(T[] value) => InvokeAll(value);
		void IParameterMatch<List<T>>.InvokeCallbacks(List<T> value) => InvokeAll(value);
		void IParameterMatch<HashSet<T>>.InvokeCallbacks(HashSet<T> value) => InvokeAll(value);
		void IParameterMatch<Queue<T>>.InvokeCallbacks(Queue<T> value) => InvokeAll(value);
		void IParameterMatch<Stack<T>>.InvokeCallbacks(Stack<T> value) => InvokeAll(value);
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
