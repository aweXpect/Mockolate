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
public static class VerificationResultExtensions
{
	extension<TMock>(VerificationResult<TMock> verificationResult)
	{
		/// <summary>
		///     …at least the expected number of <paramref name="times" />.
		/// </summary>
		public void AtLeast(int times)
		{
			IVerificationResult result = verificationResult;
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

		/// <summary>
		///     …at least once.
		/// </summary>
		public void AtLeastOnce()
		{
			IVerificationResult result = verificationResult;
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

		/// <summary>
		///     …at least twice.
		/// </summary>
		public void AtLeastTwice()
		{
			IVerificationResult result = verificationResult;
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

		/// <summary>
		///     …at most the expected number of <paramref name="times" />.
		/// </summary>
		public void AtMost(int times)
		{
			IVerificationResult result = verificationResult;
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

		/// <summary>
		///     Verifies that the mock was invoked between <paramref name="minimum" /> and <paramref name="maximum" /> times (inclusive).
		/// </summary>
		public void Between(int minimum, int maximum)
		{
			if (minimum < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(minimum), "Minimum value must be non-negative.");
			}

			if (maximum < minimum)
			{
				throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum value must be greater than or equal to minimum.");
			}

			IVerificationResult result = verificationResult;
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

		/// <summary>
		///     …at most once.
		/// </summary>
		public void AtMostOnce()
		{
			IVerificationResult result = verificationResult;
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

		/// <summary>
		///     …at most twice.
		/// </summary>
		public void AtMostTwice()
		{
			IVerificationResult result = verificationResult;
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

		/// <summary>
		///     …exactly the expected number of <paramref name="times" />.
		/// </summary>
		public void Exactly(int times)
		{
			IVerificationResult result = verificationResult;
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

		/// <summary>
		///     …never.
		/// </summary>
		public void Never()
		{
			IVerificationResult result = verificationResult;
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

		/// <summary>
		///     …exactly once.
		/// </summary>
		public void Once()
		{
			IVerificationResult result = verificationResult;
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

		/// <summary>
		///     …exactly twice.
		/// </summary>
		public void Twice()
		{
			IVerificationResult result = verificationResult;
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

		/// <summary>
		///     Verifies that the mock was invoked according to the <paramref name="predicate" />.
		/// </summary>
		public void Times(Func<int, bool> predicate, [CallerArgumentExpression("predicate")] string doNotPopulateThisValue = "")
		{
			IVerificationResult result = verificationResult;
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

		/// <summary>
		///     Supports fluent chaining of verifications in a given order.
		/// </summary>
		public void Then(params Func<IMockVerify<TMock>, VerificationResult<TMock>>[] orderedChecks)
		{
			string? error = null;
			List<string> expectations = [];
			IVerificationResult result = verificationResult;
			IMockVerify<TMock> mockVerify = GetMockVerify(((IVerificationResult<TMock>)verificationResult).Object);
			int after = -1;
			foreach (Func<IMockVerify<TMock>, VerificationResult<TMock>> check in orderedChecks)
			{
				expectations.Add(result.Expectation);
				IVerificationResult currentResult = result;
				result.Verify(interactions => VerifyInteractions(interactions, currentResult));
				result = check(mockVerify);
			}

			expectations.Add(result.Expectation);
			if (!result.Verify(interactions => VerifyInteractions(interactions, result)))
			{
				string separator = ", then ";
				throw new MockVerificationException(
					$"Expected that mock {string.Join(separator, expectations)} in order, but it {error}.");
			}

			static IMockVerify<TMock> GetMockVerify(TMock subject)
			{
				if (subject is IMockSubject<TMock> mockSubject)
				{
					return mockSubject.Mock;
				}

				throw new MockException("The subject is no mock subject.");
			}


			bool VerifyInteractions(IInteraction[] interactions, IVerificationResult currentResult)
			{
				bool hasInteractionAfter = interactions.Any(x => x.Index > after);
				after = hasInteractionAfter
					? interactions.Where(x => x.Index > after).Min(x => x.Index)
					: int.MaxValue;
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
}
