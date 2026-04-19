using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Mockolate.Exceptions;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     The expectation contains the matching interactions for verification.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public static class VerificationResultExtensions
{
	private static string ToTimes(this int amount, string verb = "")
		=> (amount, verb) switch
		{
			(0, "") => "never",
			(1, "") => "once",
			(2, "") => "twice",
			(_, "") => $"{amount} times",
			(0, _) => $"never {verb}",
			(1, _) => $"{verb} once",
			(2, _) => $"{verb} twice",
			(_, _) => $"{verb} {amount} times",
		};

	extension<TMock>(VerificationResult<TMock> verificationResult)
	{
		/// <summary>
		///     …at least the expected number of <paramref name="times" />.
		/// </summary>
		/// <exception cref="MockVerificationException">
		///     Thrown when the verified interaction occurred fewer than <paramref name="times" /> times,
		///     or when a <see cref="VerificationResult{TVerify}.Within(TimeSpan)" /> timeout elapses first.
		/// </exception>
		public void AtLeast(int times)
		{
			IVerificationResult result = verificationResult;
			try
			{
				int found = 0;
				if (!result.Verify(interactions =>
				    {
					    found = interactions.Length;
					    return interactions.Length >= times;
				    }))
				{
					throw new MockVerificationException(
						$"Expected that mock {result.Expectation} at least {times.ToTimes()}, but it {found.ToTimes("did")}.");
				}
			}
			catch (MockVerificationTimeoutException timeoutException)
			{
				throw new MockVerificationException(
					$"Expected that mock {result.Expectation} at least {times.ToTimes()}, but {timeoutException.Message}.");
			}
		}

		/// <summary>
		///     …at least once.
		/// </summary>
		/// <exception cref="MockVerificationException">
		///     Thrown when the verified interaction did not occur,
		///     or when a <see cref="VerificationResult{TVerify}.Within(TimeSpan)" /> timeout elapses first.
		/// </exception>
		public void AtLeastOnce()
		{
			IVerificationResult result = verificationResult;
			try
			{
				int found = 0;
				if (!result.Verify(interactions =>
				    {
					    found = interactions.Length;
					    return interactions.Length >= 1;
				    }))
				{
					throw new MockVerificationException(
						$"Expected that mock {result.Expectation} at least {1.ToTimes()}, but it {found.ToTimes("did")}.");
				}
			}
			catch (MockVerificationTimeoutException timeoutException)
			{
				throw new MockVerificationException(
					$"Expected that mock {result.Expectation} at least {1.ToTimes()}, but {timeoutException.Message}.");
			}
		}

		/// <summary>
		///     …at least twice.
		/// </summary>
		/// <exception cref="MockVerificationException">
		///     Thrown when the verified interaction occurred fewer than two times,
		///     or when a <see cref="VerificationResult{TVerify}.Within(TimeSpan)" /> timeout elapses first.
		/// </exception>
		public void AtLeastTwice()
		{
			IVerificationResult result = verificationResult;
			try
			{
				int found = 0;
				if (!result.Verify(interactions =>
				    {
					    found = interactions.Length;
					    return interactions.Length >= 2;
				    }))
				{
					throw new MockVerificationException(
						$"Expected that mock {result.Expectation} at least {2.ToTimes()}, but it {found.ToTimes("did")}.");
				}
			}
			catch (MockVerificationTimeoutException timeoutException)
			{
				throw new MockVerificationException(
					$"Expected that mock {result.Expectation} at least {2.ToTimes()}, but {timeoutException.Message}.");
			}
		}

		/// <summary>
		///     …at most the expected number of <paramref name="times" />.
		/// </summary>
		/// <exception cref="MockVerificationException">
		///     Thrown when the verified interaction occurred more than <paramref name="times" /> times.
		/// </exception>
		public void AtMost(int times)
		{
			IVerificationResult result = verificationResult;
			try
			{
				int found = 0;
				if (!result.Verify(interactions =>
				    {
					    found = interactions.Length;
					    return interactions.Length <= times;
				    }))
				{
					throw new MockVerificationException(
						$"Expected that mock {result.Expectation} at most {times.ToTimes()}, but it {found.ToTimes("did")}.");
				}
			}
			catch (MockVerificationTimeoutException timeoutException)
			{
				throw new MockVerificationException(
					$"Expected that mock {result.Expectation} at most {times.ToTimes()}, but {timeoutException.Message}.");
			}
		}

		/// <summary>
		///     Verifies that the mock was invoked between <paramref name="minimum" /> and <paramref name="maximum" /> times
		///     (inclusive).
		/// </summary>
		/// <exception cref="MockVerificationException">
		///     Thrown when the verified interaction count falls outside the
		///     <paramref name="minimum" />-<paramref name="maximum" /> range,
		///     or when a <see cref="VerificationResult{TVerify}.Within(TimeSpan)" /> timeout elapses first.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///     Thrown when <paramref name="minimum" /> is negative or <paramref name="maximum" /> is less than <paramref name="minimum" />.
		/// </exception>
		public void Between(int minimum, int maximum)
		{
			if (minimum < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(minimum), "Minimum value must be non-negative.");
			}

			if (maximum < minimum)
			{
				throw new ArgumentOutOfRangeException(nameof(maximum),
					"Maximum value must be greater than or equal to minimum.");
			}

			IVerificationResult result = verificationResult;
			try
			{
				int found = 0;
				if (!result.Verify(interactions =>
				    {
					    found = interactions.Length;
					    return interactions.Length >= minimum && interactions.Length <= maximum;
				    }))
				{
					throw new MockVerificationException(
						$"Expected that mock {result.Expectation} between {minimum} and {maximum} times, but it {found.ToTimes("did")}.");
				}
			}
			catch (MockVerificationTimeoutException timeoutException)
			{
				throw new MockVerificationException(
					$"Expected that mock {result.Expectation} between {minimum} and {maximum} times, but {timeoutException.Message}.");
			}
		}

		/// <summary>
		///     …at most once.
		/// </summary>
		/// <exception cref="MockVerificationException">
		///     Thrown when the verified interaction occurred more than once.
		/// </exception>
		public void AtMostOnce()
		{
			IVerificationResult result = verificationResult;
			try
			{
				int found = 0;
				if (!result.Verify(interactions =>
				    {
					    found = interactions.Length;
					    return interactions.Length <= 1;
				    }))
				{
					throw new MockVerificationException(
						$"Expected that mock {result.Expectation} at most {1.ToTimes()}, but it {found.ToTimes("did")}.");
				}
			}
			catch (MockVerificationTimeoutException timeoutException)
			{
				throw new MockVerificationException(
					$"Expected that mock {result.Expectation} at most {1.ToTimes()}, but {timeoutException.Message}.");
			}
		}

		/// <summary>
		///     …at most twice.
		/// </summary>
		/// <exception cref="MockVerificationException">
		///     Thrown when the verified interaction occurred more than two times.
		/// </exception>
		public void AtMostTwice()
		{
			IVerificationResult result = verificationResult;
			try
			{
				int found = 0;
				if (!result.Verify(interactions =>
				    {
					    found = interactions.Length;
					    return interactions.Length <= 2;
				    }))
				{
					throw new MockVerificationException(
						$"Expected that mock {result.Expectation} at most {2.ToTimes()}, but it {found.ToTimes("did")}.");
				}
			}
			catch (MockVerificationTimeoutException timeoutException)
			{
				throw new MockVerificationException(
					$"Expected that mock {result.Expectation} at most {2.ToTimes()}, but {timeoutException.Message}.");
			}
		}

		/// <summary>
		///     …exactly the expected number of <paramref name="times" />.
		/// </summary>
		/// <exception cref="MockVerificationException">
		///     Thrown when the verified interaction count is not equal to <paramref name="times" />,
		///     or when a <see cref="VerificationResult{TVerify}.Within(TimeSpan)" /> timeout elapses first.
		/// </exception>
		public void Exactly(int times)
		{
			IVerificationResult result = verificationResult;
			try
			{
				int found = 0;
				if (!result.Verify(interactions =>
				    {
					    found = interactions.Length;
					    return interactions.Length == times;
				    }))
				{
					throw new MockVerificationException(
						$"Expected that mock {result.Expectation} exactly {times.ToTimes()}, but it {found.ToTimes("did")}.");
				}
			}
			catch (MockVerificationTimeoutException timeoutException)
			{
				throw new MockVerificationException(
					$"Expected that mock {result.Expectation} exactly {times.ToTimes()}, but {timeoutException.Message}.");
			}
		}

		/// <summary>
		///     …never.
		/// </summary>
		/// <exception cref="MockVerificationException">
		///     Thrown when the verified interaction occurred at least once.
		/// </exception>
		public void Never()
		{
			IVerificationResult result = verificationResult;
			try
			{
				int found = 0;
				if (!result.Verify(interactions =>
				    {
					    found = interactions.Length;
					    return interactions.Length == 0;
				    }))
				{
					throw new MockVerificationException(
						$"Expected that mock {0.ToTimes()} {result.Expectation}, but it {found.ToTimes("did")}.");
				}
			}
			catch (MockVerificationTimeoutException timeoutException)
			{
				throw new MockVerificationException(
					$"Expected that mock {0.ToTimes()} {result.Expectation}, but {timeoutException.Message}.");
			}
		}

		/// <summary>
		///     …exactly once.
		/// </summary>
		/// <exception cref="MockVerificationException">
		///     Thrown when the verified interaction did not occur exactly once,
		///     or when a <see cref="VerificationResult{TVerify}.Within(TimeSpan)" /> timeout elapses first.
		/// </exception>
		public void Once()
		{
			IVerificationResult result = verificationResult;
			try
			{
				int found = 0;
				if (!result.Verify(interactions =>
				    {
					    found = interactions.Length;
					    return interactions.Length == 1;
				    }))
				{
					throw new MockVerificationException(
						$"Expected that mock {result.Expectation} exactly {1.ToTimes()}, but it {found.ToTimes("did")}.");
				}
			}
			catch (MockVerificationTimeoutException timeoutException)
			{
				throw new MockVerificationException(
					$"Expected that mock {result.Expectation} exactly {1.ToTimes()}, but {timeoutException.Message}.");
			}
		}

		/// <summary>
		///     …exactly twice.
		/// </summary>
		/// <exception cref="MockVerificationException">
		///     Thrown when the verified interaction did not occur exactly two times,
		///     or when a <see cref="VerificationResult{TVerify}.Within(TimeSpan)" /> timeout elapses first.
		/// </exception>
		public void Twice()
		{
			IVerificationResult result = verificationResult;
			try
			{
				int found = 0;
				if (!result.Verify(interactions =>
				    {
					    found = interactions.Length;
					    return interactions.Length == 2;
				    }))
				{
					throw new MockVerificationException(
						$"Expected that mock {result.Expectation} exactly {2.ToTimes()}, but it {found.ToTimes("did")}.");
				}
			}
			catch (MockVerificationTimeoutException timeoutException)
			{
				throw new MockVerificationException(
					$"Expected that mock {result.Expectation} exactly {2.ToTimes()}, but {timeoutException.Message}.");
			}
		}

		/// <summary>
		///     Verifies that the mock was invoked according to the <paramref name="predicate" />.
		/// </summary>
		/// <param name="predicate">
		///     Receives the actual number of matching interactions and returns <see langword="true" /> if that count is acceptable.
		/// </param>
		/// <param name="doNotPopulateThisValue">
		///     Populated by the compiler via <see cref="System.Runtime.CompilerServices.CallerArgumentExpressionAttribute" />
		///     to include the source expression of <paramref name="predicate" /> in failure messages.
		/// </param>
		/// <exception cref="MockVerificationException">
		///     Thrown when <paramref name="predicate" /> returns <see langword="false" /> for the observed count,
		///     or when a <see cref="VerificationResult{TVerify}.Within(TimeSpan)" /> timeout elapses first.
		/// </exception>
		public void Times(Func<int, bool> predicate,
			[CallerArgumentExpression("predicate")]
			string doNotPopulateThisValue = "")
		{
			IVerificationResult result = verificationResult;
			try
			{
				int found = 0;
				if (!result.Verify(interactions =>
				    {
					    found = interactions.Length;
					    return predicate(interactions.Length);
				    }))
				{
					throw new MockVerificationException(
						$"Expected that mock {result.Expectation} according to the predicate {doNotPopulateThisValue}, but it {found.ToTimes("did")}.");
				}
			}
			catch (MockVerificationTimeoutException timeoutException)
			{
				throw new MockVerificationException(
					$"Expected that mock {result.Expectation} according to the predicate {doNotPopulateThisValue}, but {timeoutException.Message}.");
			}
		}

		/// <summary>
		///     Supports fluent chaining of verifications in a given order.
		/// </summary>
		/// <param name="orderedChecks">
		///     Each callback returns a follow-up <see cref="VerificationResult{TVerify}" />; that interaction must have been
		///     recorded strictly after the previous verification in the chain for the assertion to pass.
		/// </param>
		/// <exception cref="MockVerificationException">
		///     Thrown when the expected interactions did not occur in the given order
		///     (for example, a later interaction was recorded before an earlier one, or was missing).
		/// </exception>
		public void Then(params Func<TMock, VerificationResult<TMock>>[] orderedChecks)
		{
			string? error = null;
			List<string> expectations = [];
			IVerificationResult result = verificationResult;
			TMock mockVerify = ((IVerificationResult<TMock>)verificationResult).Object;
			IInteraction[] snapshot = result.MockInteractions.ToArray();
			Dictionary<IInteraction, int> positions = new(snapshot.Length);
			for (int i = 0; i < snapshot.Length; i++)
			{
				positions[snapshot[i]] = i;
			}

			int after = -1;
			foreach (Func<TMock, VerificationResult<TMock>> check in orderedChecks)
			{
				expectations.Add(result.Expectation);
				IVerificationResult currentResult = result;
				// In case of an error, `after` is set to int.MaxValue and all following verifications will fail
				_ = result.Verify(interactions => VerifyInteractions(interactions, currentResult));
				result = check(mockVerify);
			}

			expectations.Add(result.Expectation);
			if (!result.Verify(interactions => VerifyInteractions(interactions, result)))
			{
				string separator = ", then ";
				throw new MockVerificationException(
					$"Expected that mock {string.Join(separator, expectations)} in order, but it {error}.");
			}

			bool VerifyInteractions(IInteraction[] interactions, IVerificationResult currentResult)
			{
				int bestPosition = int.MaxValue;
				IInteraction? firstInteraction = null;
				foreach (IInteraction candidate in interactions)
				{
					if (positions.TryGetValue(candidate, out int position) &&
					    position > after &&
					    position < bestPosition)
					{
						bestPosition = position;
						firstInteraction = candidate;
					}
				}

				bool hasInteractionAfter = firstInteraction is not null;
				after = hasInteractionAfter ? bestPosition : int.MaxValue;
				if (!hasInteractionAfter && error is null)
				{
					error = interactions.Length > 0
						? $"{currentResult.Expectation} too early"
						: $"{currentResult.Expectation} not at all";
				}

				return hasInteractionAfter;
			}
		}
	}
}
