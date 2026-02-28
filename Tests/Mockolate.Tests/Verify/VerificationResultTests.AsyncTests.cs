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
		public async Task WithCancellationToken_ShouldIncludeTimeoutInException()
		{
			IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
			using CancellationTokenSource cts = new(100);
			CancellationToken token = cts.Token;

			void Act()
			{
				sut.VerifyMock.Invoked.Dispense(Match.AnyParameters()).WithCancellation(token).AtLeastOnce();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage(
					"Expected that mock invoked method Dispense(Match.AnyParameters()) at least once, but it timed out.");
		}

		[Fact]
		public async Task Within_ShouldAbortAsSoonAsConditionIsSatisfied()
		{
			IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
			using CancellationTokenSource cts = new();
			CancellationToken token = cts.Token;

			_ = Task.Run(async () =>
			{
				for (int i = 0; i < 100; i++)
				{
					await Task.Delay(100);
					sut.Dispense("Dark", i);
					if (token.IsCancellationRequested)
					{
						break;
					}
				}
			}, token);

			Stopwatch sw = Stopwatch.StartNew();
			sut.VerifyMock.Invoked.Dispense(Match.AnyParameters()).Within(TimeSpan.FromMilliseconds(500))
				.AtLeastOnce();
			sw.Stop();
			cts.Cancel();

			await That(sw.Elapsed).IsLessThan(TimeSpan.FromSeconds(1));
		}

		[Fact]
		public async Task WithTimeout_ShouldIncludeTimeoutInException()
		{
			IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

			void Act()
			{
				sut.VerifyMock.Invoked.Dispense(Match.AnyParameters()).Within(TimeSpan.FromMilliseconds(100))
					.AtLeastOnce();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage(
					"Expected that mock invoked method Dispense(Match.AnyParameters()) at least once, but it timed out after 00:00:00.1000000.");
		}
	}
}
