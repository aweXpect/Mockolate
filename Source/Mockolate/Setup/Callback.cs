using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Mockolate.Setup;

/// <summary>
///     A callback wrapper that allows conditional invocation based on predicates.
/// </summary>
public class Callback
{
	private int? _forTimes;
	private Func<int, bool>? _invocationPredicate;
	private int _onlyTimes;

	/// <summary>
	///     Specifies if the callback may be invoked in parallel.
	/// </summary>
	protected bool RunInParallel { get; private set; }

	/// <summary>
	///     Check if a <see cref="For" />-predicate was specified.
	/// </summary>
	protected bool HasForSpecified => _forTimes != null;

	/// <summary>
	///     Flag indicating whether the callback is active.
	/// </summary>
	/// <remarks>
	///     A <see cref="Callback" /> is active, until it has reached the <see cref="Only" /> limit, if specified.
	/// </remarks>
	protected bool IsActive(int matchingCount)
		=> _onlyTimes == 0 || matchingCount < _onlyTimes * (_forTimes ?? 1);

	/// <summary>
	///     Limits the callback to only execute for property accesses where the predicate returns <see langword="true" />.
	/// </summary>
	/// <remarks>
	///     Provides a zero-based counter indicating how many times the property has been accessed so far.
	/// </remarks>
	public void When(Func<int, bool> predicate)
		=> _invocationPredicate = predicate;

	/// <summary>
	///     Repeats the callback for the given number of <paramref name="times" />.
	///     <see langword="true" />.
	/// </summary>
	/// <remarks>
	///     The number of times is only counted for actual executions (<see cref="When" /> evaluates
	///     to <see langword="true" />).
	/// </remarks>
	public void For(int times)
	{
		if (times <= 0)
		{
			// ReSharper disable once LocalizableElement
			throw new ArgumentOutOfRangeException(nameof(times), "Times must be greater than zero.");
		}

		_forTimes = times;
	}

	/// <summary>
	///     Deactivates the callback after it was invoked the given number of <paramref name="times" />.
	/// </summary>
	public void Only(int times)
	{
		if (times <= 0)
		{
			// ReSharper disable once LocalizableElement
			throw new ArgumentOutOfRangeException(nameof(times), "Times must be greater than zero.");
		}

		_onlyTimes = times;
	}

	/// <summary>
	///     Specifies that the callback may be invoked in parallel.
	/// </summary>
	public void InParallel()
		=> RunInParallel = true;

	/// <summary>
	///     Check if the invocation count satisfies the <see cref="When" />-predicate.
	/// </summary>
	protected bool CheckInvocations(int invocationCount)
		=> _invocationPredicate?.Invoke(invocationCount) ?? true;

	/// <summary>
	///     Check if the callback should be repeated (at least) once more.
	/// </summary>
	protected bool CheckMatching(int matchingCount)
		=> _forTimes is null || matchingCount < _forTimes;
}

/// <summary>
///     A callback wrapper for the <paramref name="delegate" /> that allows conditional invocation based on predicates.
/// </summary>
public class Callback<TDelegate>(TDelegate @delegate) : Callback where TDelegate : Delegate
{
	private int _forIterationCount;
	private int _invocationCount;
	private int _matchingCount;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
	/// <summary>
	///     Invokes the callback if the predicates are satisfied, providing the invocation count.
	/// </summary>
	public bool Invoke(bool wasInvoked, ref int index, Action<int, TDelegate> callback)
	{
		if (IsActive(_matchingCount) && CheckInvocations(_invocationCount))
		{
			if (CheckMatching(_forIterationCount))
			{
				_invocationCount++;

				if (RunInParallel)
				{
					if (!wasInvoked)
					{
						Interlocked.Increment(ref index);
					}

					_forIterationCount++;
					_matchingCount++;
					callback(_invocationCount - 1, @delegate);
				}
				else if (!wasInvoked)
				{
					if (!HasForSpecified || !CheckMatching(_forIterationCount + 1))
					{
						Interlocked.Increment(ref index);
						_forIterationCount = 0;
					}
					else
					{
						_forIterationCount++;
					}

					_matchingCount++;
					callback(_invocationCount - 1, @delegate);
				}

				return !RunInParallel;
			}
		}

		_invocationCount++;
		return false;
	}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high

	/// <summary>
	///     Invokes the callback if the predicates are satisfied, providing the invocation count.
	/// </summary>
	public bool Invoke(ref int index, Action<int, TDelegate> callback)
	{
		if (IsActive(_matchingCount) && CheckInvocations(_invocationCount))
		{
			if (CheckMatching(_forIterationCount))
			{
				if (!HasForSpecified || !CheckMatching(_forIterationCount + 1))
				{
					Interlocked.Increment(ref index);
					_forIterationCount = 0;
				}
				else
				{
					_forIterationCount++;
				}

				_invocationCount++;
				_matchingCount++;
				callback(_invocationCount - 1, @delegate);
				return true;
			}
		}

		_invocationCount++;
		Interlocked.Increment(ref index);
		return false;
	}

	/// <summary>
	///     Invokes the callback if the predicates are satisfied, providing the invocation count.
	/// </summary>
	public bool Invoke<TReturn>(ref int index, Func<int, TDelegate, TReturn> callback,
		[NotNullWhen(true)] out TReturn? returnValue)
	{
		if (IsActive(_matchingCount) && CheckInvocations(_invocationCount))
		{
			if (CheckMatching(_forIterationCount))
			{
				if (!HasForSpecified || !CheckMatching(_forIterationCount + 1))
				{
					Interlocked.Increment(ref index);
					_forIterationCount = 0;
				}
				else
				{
					_forIterationCount++;
				}

				_invocationCount++;
				_matchingCount++;
				returnValue = callback(_invocationCount - 1, @delegate)!;
				return true;
			}
		}

		_invocationCount++;
		returnValue = default;
		Interlocked.Increment(ref index);
		return false;
	}
}
