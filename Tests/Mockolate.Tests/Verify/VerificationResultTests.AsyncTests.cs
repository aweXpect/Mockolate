using System.Diagnostics;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public sealed partial class VerificationResultTests
{
	public class AsyncTests
	{
		[Fact]
		public async Task MultipleWithin_ShouldOverwritePreviousTimeout()
		{
			IChocolateDispenser sut = IChocolateDispenser.CreateMock();

			void Act()
			{
				sut.Mock.Verify.Dispense(Match.AnyParameters())
					.Within(TimeSpan.FromMilliseconds(100))
					.Within(TimeSpan.FromMilliseconds(200))
					.AtLeastOnce();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage(
					"Expected that mock invoked method Dispense(Match.AnyParameters()) at least once, but it timed out after 00:00:00.2000000.");
		}

		[Fact]
		public async Task VerifyAsync_WhenAlreadySuccessful_ShouldReturnTrue()
		{
			IChocolateDispenser sut = IChocolateDispenser.CreateMock();
			sut.Dispense("Dark", 1);
			sut.Dispense("Dark", 2);

			VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result = sut.Mock.Verify.Dispense(Match.AnyParameters())
				.Within(TimeSpan.FromMilliseconds(500));

			await That(((IAsyncVerificationResult)result).VerifyAsync(l => l.Length > 0)).IsTrue();
		}

		[Fact]
		public async Task VerifyAsync_WhenMultipleIterationsAreNecessary_ShouldStopWhenSuccessful()
		{
			IChocolateDispenser sut = IChocolateDispenser.CreateMock();

			VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result = sut.Mock.Verify.Dispense(Match.AnyParameters())
				.Within(TimeSpan.FromSeconds(30));
			using CancellationTokenSource cts = new();
			CancellationToken token = cts.Token;

			Task backgroundTask = Task.Run(async () =>
			{
				for (int i = 0; i < 1000; i++)
				{
					await Task.Delay(10, CancellationToken.None).ConfigureAwait(false);
					sut.Dispense("Dark", i);
					if (token.IsCancellationRequested)
					{
						break;
					}
				}
			}, token);

			await That(((IAsyncVerificationResult)result).VerifyAsync(l => l.Length > 20)).IsTrue();
			cts.Cancel();
			await backgroundTask;
		}

		[Fact]
		public async Task WithCancellation_ShouldReturnAsyncVerificationResult()
		{
			IChocolateDispenser sut = IChocolateDispenser.CreateMock();

			VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result = sut.Mock.Verify.Dispense(Match.AnyParameters())
				.WithCancellation(CancellationToken.None);

			await That(result).Is<IAsyncVerificationResult>();
		}

		[Fact]
		public async Task WithCancellationAndTimeout_ShouldCombineBoth()
		{
			IChocolateDispenser sut = IChocolateDispenser.CreateMock();
			using CancellationTokenSource cts = new(50);
			CancellationToken token = cts.Token;

			void Act()
			{
				sut.Mock.Verify.Dispense(Match.AnyParameters())
					.Within(TimeSpan.FromMilliseconds(30000))
					.WithCancellation(token)
					.AtLeastOnce();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage(
					"Expected that mock invoked method Dispense(Match.AnyParameters()) at least once, but it timed out.");
		}

		[Fact]
		public async Task WithCancellationAndTimeout_ShouldIncludeTimeoutInExceptionWhenLessThanCancellationToken()
		{
			IChocolateDispenser sut = IChocolateDispenser.CreateMock();
			using CancellationTokenSource cts = new(30000);
			CancellationToken token = cts.Token;

			void Act()
			{
				sut.Mock.Verify.Dispense(Match.AnyParameters())
					.Within(TimeSpan.FromMilliseconds(50))
					.WithCancellation(token)
					.AtLeastOnce();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage(
					"Expected that mock invoked method Dispense(Match.AnyParameters()) at least once, but it timed out after 00:00:00.0500000.");
		}

		[Fact]
		public async Task WithCancellationToken_ShouldIncludeTimeoutInException()
		{
			IChocolateDispenser sut = IChocolateDispenser.CreateMock();
			using CancellationTokenSource cts = new(100);
			CancellationToken token = cts.Token;

			void Act()
			{
				sut.Mock.Verify.Dispense(Match.AnyParameters()).WithCancellation(token).AtLeastOnce();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage(
					"Expected that mock invoked method Dispense(Match.AnyParameters()) at least once, but it timed out.");
		}

		[Fact]
		public async Task Within_ShouldAbortAsSoonAsConditionIsSatisfied()
		{
			IChocolateDispenser sut = IChocolateDispenser.CreateMock();
			using CancellationTokenSource cts = new();
			CancellationToken token = cts.Token;

			Task backgroundTask = Task.Run(async () =>
			{
				for (int i = 0; i < 100; i++)
				{
					await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);
					sut.Dispense("Dark", i);
					if (token.IsCancellationRequested)
					{
						break;
					}
				}
			}, token);

			Stopwatch sw = Stopwatch.StartNew();
			sut.Mock.Verify.Dispense(Match.AnyParameters()).Within(TimeSpan.FromMilliseconds(500))
				.AtLeastOnce();
			sw.Stop();
			cts.Cancel();
			await backgroundTask;

			await That(sw.Elapsed).IsLessThan(TimeSpan.FromSeconds(2));
		}

		[Fact]
		public async Task Within_ShouldIncludeTimeoutInException()
		{
			IChocolateDispenser sut = IChocolateDispenser.CreateMock();

			void Act()
			{
				sut.Mock.Verify.Dispense(Match.AnyParameters()).Within(TimeSpan.FromMilliseconds(100))
					.AtLeastOnce();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage(
					"Expected that mock invoked method Dispense(Match.AnyParameters()) at least once, but it timed out after 00:00:00.1000000.");
		}

		[Fact]
		public async Task Within_ShouldReturnAsyncVerificationResult()
		{
			IChocolateDispenser sut = IChocolateDispenser.CreateMock();

			VerificationResult<Mock.IMockVerifyForIChocolateDispenser> result = sut.Mock.Verify.Dispense(Match.AnyParameters())
				.Within(TimeSpan.FromMilliseconds(100));

			await That(result).Is<IAsyncVerificationResult>();
		}

		[Fact]
		public async Task Within_WhenInvokedMultipleTimesInBackground_ShouldNotThrow()
		{
			IChocolateDispenser mock = IChocolateDispenser.CreateMock();

			Task backgroundTask = Task.Delay(50, CancellationToken.None)
				.ContinueWith(_ =>
				{
					for (int i = 0; i < 15; i++)
					{
						mock.Dispense("dark", i);
					}
				}, CancellationToken.None);

			mock.Mock.Verify.Dispense(Match.AnyParameters()).Within(TimeSpan.FromSeconds(30)).AtLeast(8);

			await backgroundTask;
		}
	}
}
