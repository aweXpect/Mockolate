using System;
using System.Collections.Generic;
using System.Linq;
using Mockolate.Exceptions;
using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     The expectation contains the matching interactions for verification.
/// </summary>
public static class VerificationResultExtensions
{
	/// <summary>
	///     …at least the expected number of <paramref name="times" />.
	/// </summary>
	public static void AtLeast<TMock>(this VerificationResult<TMock> verificationResult, int times)
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
	public static void AtLeastOnce<TMock>(this VerificationResult<TMock> verificationResult)
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
	public static void AtLeastTwice<TMock>(this VerificationResult<TMock> verificationResult)
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
	public static void AtMost<TMock>(this VerificationResult<TMock> verificationResult, int times)
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
	///     …at most once.
	/// </summary>
	public static void AtMostOnce<TMock>(this VerificationResult<TMock> verificationResult)
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
	public static void AtMostTwice<TMock>(this VerificationResult<TMock> verificationResult)
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
	public static void Exactly<TMock>(this VerificationResult<TMock> verificationResult, int times)
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
	public static void Never<TMock>(this VerificationResult<TMock> verificationResult)
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
	public static void Once<TMock>(this VerificationResult<TMock> verificationResult)
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
	public static void Twice<TMock>(this VerificationResult<TMock> verificationResult)
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
	///     Supports fluent chaining of verifications in a given order.
	/// </summary>
	public static void Then<T>(this VerificationResult<T> verificationResult,
		params Func<IMockVerify<T>, VerificationResult<T>>[] orderedChecks)
	{
		string? error = null;
		bool flag = true;
		List<string> expectations = [];
		IVerificationResult result = verificationResult;
		IMockVerify<T> mockVerify = GetMockVerify(((IVerificationResult<T>)verificationResult).Object);
		int after = -1;
		foreach (Func<IMockVerify<T>, VerificationResult<T>>? check in orderedChecks)
		{
			expectations.Add(result.Expectation);
			if (!result.Verify(VerifyInteractions))
			{
				flag = false;
			}

			result = check(mockVerify);
		}

		expectations.Add(result.Expectation);
		if (!result.Verify(VerifyInteractions) || !flag)
		{
			string? separator = ", then ";
			throw new MockVerificationException(
				$"Expected that mock {string.Join(separator, expectations)} in order, but it {error}.");
		}

		static IMockVerify<T> GetMockVerify(T subject)
		{
			if (subject is IMockSubject<T> mockSubject)
			{
				return mockSubject.Mock;
			}

			if (subject is IHasMockRegistration hasMockRegistration)
			{
				return new Mock<T>(subject, hasMockRegistration.Registrations);
			}

			throw new MockException("The subject is no mock subject.");
		}

		bool VerifyInteractions(IInteraction[] interactions)
		{
			bool hasInteractionAfter = interactions.Any(x => x.Index > after);
			after = hasInteractionAfter
				? interactions.Where(x => x.Index > after).Min(x => x.Index)
				: int.MaxValue;
			if (!hasInteractionAfter && error is null)
			{
				error = interactions.Length > 0
					? $"{result.Expectation} too early"
					: $"{result.Expectation} not at all";
			}

			return hasInteractionAfter;
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
