using System;
using System.Threading;

namespace Mockolate.Setup;

/// <summary>
///     A callback wrapper that allows conditional invocation based on predicates.
/// </summary>
public class Callback
{
	private Func<int, bool>? _invocationPredicate;
	private Func<int, bool>? _matchingPredicate;

	/// <summary>
	///     Check if a <see cref="For" />-predicate was specified.
	/// </summary>
	protected bool HasForSpecified => _matchingPredicate != null;

	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns <see langword="true" />.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	public void When(Func<int, bool> predicate)
		=> _invocationPredicate = predicate;

	/// <summary>
	///     Limits the callback to only execute for matching property accesses where the predicate returns
	///     <see langword="true" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (<see cref="When" /> evaluates to <see langword="true" />
	///     ).
	/// </remarks>
	public void For(Func<int, bool> predicate)
		=> _matchingPredicate = predicate;

	/// <summary>
	///     Check if the invocation count satisfies the <see cref="When" />-predicate.
	/// </summary>
	protected bool CheckInvocations(int invocationCount)
		=> _invocationPredicate?.Invoke(invocationCount) ?? true;

	/// <summary>
	///     Check if the invocation count satisfies the <see cref="For" />-predicate.
	/// </summary>
	protected bool CheckMatching(int matchingCount)
		=> _matchingPredicate?.Invoke(matchingCount) ?? true;
}

/// <summary>
///     A callback wrapper for the <paramref name="delegate" /> that allows conditional invocation based on predicates.
/// </summary>
public class Callback<TDelegate>(TDelegate @delegate) : Callback where TDelegate : Delegate
{
	private int _invocationCount;
	private int _matchingCount;

	/// <summary>
	///     Invokes the callback if the predicates are satisfied, providing the invocation count.
	/// </summary>
	public void Invoke(Action<int, TDelegate> callback)
	{
		if (CheckInvocations(_invocationCount))
		{
			_invocationCount++;
			if (CheckMatching(_matchingCount))
			{
				_matchingCount++;
				callback(_invocationCount - 1, @delegate);
			}
			else
			{
				_matchingCount++;
			}
		}
		else
		{
			_invocationCount++;
		}
	}

	/// <summary>
	///     Invokes the callback if the predicates are satisfied, providing the invocation count.
	/// </summary>
	public bool Invoke(ref int index, Action<int, TDelegate> callback)
	{
		if (CheckInvocations(_invocationCount))
		{
			if (CheckMatching(_matchingCount))
			{
				if (!HasForSpecified || !CheckMatching(_matchingCount + 1))
				{
					Interlocked.Increment(ref index);
				}

				_invocationCount++;
				_matchingCount++;
				callback(_invocationCount - 1, @delegate);
				return true;
			}

			_matchingCount++;
		}

		_invocationCount++;
		Interlocked.Increment(ref index);
		return false;
	}

	/// <summary>
	///     Invokes the callback if the predicates are satisfied, providing the invocation count.
	/// </summary>
	public bool Invoke<TReturn>(ref int index, Func<int, TDelegate, TReturn> callback, out TReturn? returnValue)
	{
		if (CheckInvocations(_invocationCount))
		{
			if (CheckMatching(_matchingCount))
			{
				if (!HasForSpecified || !CheckMatching(_matchingCount + 1))
				{
					Interlocked.Increment(ref index);
				}

				_invocationCount++;
				_matchingCount++;
				returnValue = callback(_invocationCount - 1, @delegate)!;

				return true;
			}

			_matchingCount++;
		}

		_invocationCount++;
		returnValue = default;
		Interlocked.Increment(ref index);
		return false;
	}
}
