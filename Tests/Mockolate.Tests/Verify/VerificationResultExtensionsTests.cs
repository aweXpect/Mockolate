using System.Collections.Generic;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public class VerificationResultExtensionsTests
{
	[Test]
	[Arguments(0, 0, true)]
	[Arguments(2, 3, false)]
	[Arguments(2, 2, true)]
	[Arguments(2, 1, true)]
	public async Task AtLeast_ShouldReturnExpectedResult(int count, int times, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).AtLeast(times);
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) at least 3 times, but it did twice.");
	}

	[Test]
	public async Task AtLeast_WhenTimedOut_ShouldThrowMockVerificationException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
				.Within(TimeSpan.FromMilliseconds(20)).AtLeast(5);
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) at least 5 times, but it timed out after 00:00:00.0200000.");
	}

	[Test]
	[Arguments(0, false)]
	[Arguments(1, true)]
	[Arguments(2, true)]
	[Arguments(3, true)]
	public async Task AtLeastOnce_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).AtLeastOnce();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) at least once, but it never did.");
	}

	[Test]
	public async Task AtLeastOnce_WhenTimedOut_ShouldThrowMockVerificationException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
				.Within(TimeSpan.FromMilliseconds(20)).AtLeastOnce();
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) at least once, but it timed out after 00:00:00.0200000.");
	}

	[Test]
	[Arguments(0, false)]
	[Arguments(1, false)]
	[Arguments(2, true)]
	[Arguments(3, true)]
	public async Task AtLeastTwice_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).AtLeastTwice();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) at least twice, but it {count switch { 0 => "never did", _ => "did once", }}.");
	}

	[Test]
	public async Task AtLeastTwice_WhenTimedOut_ShouldThrowMockVerificationException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
				.Within(TimeSpan.FromMilliseconds(20)).AtLeastTwice();
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) at least twice, but it timed out after 00:00:00.0200000.");
	}

	[Test]
	[Arguments(0, 0, true)]
	[Arguments(2, 1, false)]
	[Arguments(2, 2, true)]
	[Arguments(2, 3, true)]
	public async Task AtMost_ShouldReturnExpectedResult(int count, int times, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).AtMost(times);
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) at most once, but it did twice.");
	}

	[Test]
	public async Task AtMost_WhenTimedOut_ShouldThrowMockVerificationException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, 5);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
				.Within(TimeSpan.FromMilliseconds(20)).AtMost(4);
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) at most 4 times, but it timed out after 00:00:00.0200000.");
	}

	[Test]
	[Arguments(0, true)]
	[Arguments(1, true)]
	[Arguments(2, false)]
	[Arguments(3, false)]
	public async Task AtMostOnce_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).AtMostOnce();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) at most once, but it did {(count == 2 ? "twice" : $"{count} times")}.");
	}

	[Test]
	public async Task AtMostOnce_WhenTimedOut_ShouldThrowMockVerificationException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, 2);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
				.Within(TimeSpan.FromMilliseconds(20)).AtMostOnce();
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) at most once, but it timed out after 00:00:00.0200000.");
	}

	[Test]
	[Arguments(0, true)]
	[Arguments(1, true)]
	[Arguments(2, true)]
	[Arguments(3, false)]
	public async Task AtMostTwice_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).AtMostTwice();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) at most twice, but it did {count} times.");
	}

	[Test]
	public async Task AtMostTwice_WhenTimedOut_ShouldThrowMockVerificationException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, 3);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
				.Within(TimeSpan.FromMilliseconds(20)).AtMostTwice();
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) at most twice, but it timed out after 00:00:00.0200000.");
	}

	[Test]
	[Arguments(0, 0, 0, true)]
	[Arguments(1, 0, 2, true)]
	[Arguments(2, 0, 2, true)]
	[Arguments(3, 2, 5, true)]
	[Arguments(1, 2, 5, false)]
	[Arguments(6, 2, 5, false)]
	[Arguments(2, 2, 2, true)]
	public async Task Between_ShouldReturnExpectedResult(int count, int minimum, int maximum, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).Between(minimum, maximum);
		}

		string expectedDidTimes = count switch
		{
			0 => "never did",
			1 => "did once",
			2 => "did twice",
			_ => $"did {count} times",
		};

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) between {minimum} and {maximum} times, but it {expectedDidTimes}.");
	}

	[Test]
	public async Task Between_WhenTimedOut_ShouldThrowMockVerificationException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
				.Within(TimeSpan.FromMilliseconds(20)).Between(3, 5);
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) between 3 and 5 times, but it timed out after 00:00:00.0200000.");
	}

	[Test]
	public async Task Between_WithMaximumLessThanMinimum_ShouldThrowArgumentOutOfRangeException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).Between(5, 2);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("maximum").And
			.WithMessage("Maximum value must be greater than or equal to minimum.").AsPrefix();
	}

	[Test]
	public async Task Between_WithNegativeMinimum_ShouldThrowArgumentOutOfRangeException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).Between(-1, 5);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("minimum").And
			.WithMessage("Minimum value must be non-negative.").AsPrefix();
	}

	[Test]
	[Arguments(0, 0, true)]
	[Arguments(2, 3, false)]
	[Arguments(2, 2, true)]
	[Arguments(2, 1, false)]
	public async Task Exactly_ShouldReturnExpectedResult(int count, int times, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).Exactly(times);
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) exactly {(times == 1 ? "once" : $"{times} times")}, but it did twice.");
	}

	[Test]
	public async Task Exactly_WhenTimedOut_ShouldThrowMockVerificationException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
				.Within(TimeSpan.FromMilliseconds(20)).Exactly(3);
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) exactly 3 times, but it timed out after 00:00:00.0200000.");
	}

	[Test]
	[Arguments(0, true)]
	[Arguments(1, false)]
	[Arguments(2, false)]
	[Arguments(3, false)]
	public async Task Never_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).Never();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock never invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()), but it did {count switch { 1 => "once", 2 => "twice", _ => $"{count} times", }}.");
	}

	[Test]
	public async Task Never_WhenTimedOut_ShouldThrowMockVerificationException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, 1);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
				.Within(TimeSpan.FromMilliseconds(20)).Never();
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock never invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()), but it timed out after 00:00:00.0200000.");
	}

	[Test]
	[Arguments(0, false)]
	[Arguments(1, true)]
	[Arguments(2, false)]
	[Arguments(3, false)]
	public async Task Once_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).Once();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) exactly once, but it {count switch { 0 => "never did", 2 => "did twice", _ => $"did {count} times", }}.");
	}

	[Test]
	public async Task Once_WhenTimedOut_ShouldThrowMockVerificationException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
				.Within(TimeSpan.FromMilliseconds(20)).Once();
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) exactly once, but it timed out after 00:00:00.0200000.");
	}

	[Test]
	public async Task Then_ShouldVerifyInOrder()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		mock.Dispense("Dark", 1);
		mock.Dispense("Dark", 2);
		mock.Dispense("Dark", 3);
		mock.Dispense("Dark", 4);

		mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.Is(3))
			.Then(m => m.Invoked.Dispense(It.IsAny<string>(), It.Is(4)));

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.Is(2))
				.Then(m => m.Invoked.Dispense(It.IsAny<string>(), It.Is(1)));
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), 2), then invoked method Dispense(It.IsAny<string>(), 1) in order, but it invoked method Dispense(It.IsAny<string>(), 1) too early.");
		mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.Is(1))
			.Then(m => m.Invoked.Dispense(It.IsAny<string>(), It.Is(2)));
	}

	[Test]
	[Arguments(false, 1, 2, 3, 4)]
	[Arguments(true, 1, 2, 2, 4)]
	[Arguments(true, 1, 2, 3, 2, 4)]
	public async Task Then_TwiceSame_ShouldOnlyCountOnce(bool expectMatch, params int[] values)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		foreach (int value in values)
		{
			mock.Dispense("Dark", value);
		}


		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.Is(2))
				.Then(m => m.Invoked.Dispense(It.IsAny<string>(), It.Is(2)));
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectMatch);
	}

	[Test]
	public async Task Then_WhenNoMatch_ShouldFail()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		mock.Dispense("Dark", 1);
		mock.Dispense("Dark", 2);
		mock.Dispense("Dark", 3);
		mock.Dispense("Dark", 4);

		await That(void () => mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.Is(6))
				.Then(m => m.Invoked.Dispense(It.IsAny<string>(), It.Is(4))))
			.Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), 6), then invoked method Dispense(It.IsAny<string>(), 4) in order, but it invoked method Dispense(It.IsAny<string>(), 6) not at all.");

		await That(void () => mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.Is(1))
				.Then(m => m.Invoked.Dispense(It.IsAny<string>(), It.Is(6)),
					m => m.Invoked.Dispense(It.IsAny<string>(), It.Is(3))))
			.Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), 1), then invoked method Dispense(It.IsAny<string>(), 6), then invoked method Dispense(It.IsAny<string>(), 3) in order, but it invoked method Dispense(It.IsAny<string>(), 6) not at all.");

		await That(void () => mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.Is(1))
				.Then(m => m.Invoked.Dispense(It.IsAny<string>(), It.Is(2)),
					m => m.Invoked.Dispense(It.IsAny<string>(), It.Is(6))))
			.Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), 1), then invoked method Dispense(It.IsAny<string>(), 2), then invoked method Dispense(It.IsAny<string>(), 6) in order, but it invoked method Dispense(It.IsAny<string>(), 6) not at all.");
	}

	[Test]
	public async Task Then_WhenNoMock_ShouldThrowMockException()
	{
		IChocolateDispenser mock = new MyChocolateDispenser();
		VerificationResult<IChocolateDispenser> result = new(mock, new MockInteractions(), _ => false, "foo");

		void Act()
		{
			result.Then(m => m.Invoked.Dispense(It.IsAny<string>(), It.Is(1)));
		}

		await That(Act).Throws<MockException>()
			.WithMessage("The subject is no mock subject.");
	}

	[Test]
	public async Task Then_WhenOnlyPartlyMatch_ShouldFail()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		mock.Dispense("Dark", 1);
		mock.Dispense("Dark", 2);
		mock.Dispense("Dark", 3);
		mock.Dispense("Dark", 4);

		await That(void () => mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.Is(2))
				.Then(m => m.Invoked.Dispense(It.IsAny<string>(), It.Is(1)),
					m => m.Invoked.Dispense(It.IsAny<string>(), It.Is(4))))
			.Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), 2), then invoked method Dispense(It.IsAny<string>(), 1), then invoked method Dispense(It.IsAny<string>(), 4) in order, but it invoked method Dispense(It.IsAny<string>(), 1) too early.");
	}

	[Test]
	public async Task Times_WhenTimedOut_ShouldThrowMockVerificationException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
				.Within(TimeSpan.FromMilliseconds(20)).Times(_ => false);
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) according to the predicate _ => false, but it timed out after 00:00:00.0200000.");
	}

	[Test]
	[Arguments(0, true)]
	[Arguments(1, false)]
	[Arguments(2, true)]
	[Arguments(3, false)]
	[Arguments(4, true)]
	[Arguments(5, false)]
	public async Task Times_WithEvenPredicate_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).Times(n => n % 2 == 0);
		}

		string expectedDidTimes = count switch
		{
			0 => "never did",
			1 => "did once",
			2 => "did twice",
			_ => $"did {count} times",
		};

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) according to the predicate n => n % 2 == 0, but it {expectedDidTimes}.");
	}

	[Test]
	[Arguments(0, false)]
	[Arguments(1, false)]
	[Arguments(2, true)]
	[Arguments(3, true)]
	[Arguments(4, false)]
	[Arguments(5, true)]
	[Arguments(6, false)]
	[Arguments(7, true)]
	public async Task Times_WithPrimePredicate_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).Times(IsPrime);
		}

		string expectedDidTimes = count switch
		{
			0 => "never did",
			1 => "did once",
			2 => "did twice",
			_ => $"did {count} times",
		};

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) according to the predicate IsPrime, but it {expectedDidTimes}.");

		static bool IsPrime(int number)
		{
			if (number <= 1)
			{
				return false;
			}

			if (number == 2)
			{
				return true;
			}

			if (number % 2 == 0)
			{
				return false;
			}

			int boundary = (int)Math.Floor(Math.Sqrt(number));
			for (int i = 3; i <= boundary; i += 2)
			{
				if (number % i == 0)
				{
					return false;
				}
			}

			return true;
		}
	}

	[Test]
	[Arguments(0, false)]
	[Arguments(1, false)]
	[Arguments(2, true)]
	[Arguments(3, false)]
	public async Task Twice_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>()).Twice();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) exactly twice, but it {count switch { 0 => "never did", 1 => "did once", _ => $"did {count} times", }}.");
	}

	[Test]
	public async Task Twice_WhenTimedOut_ShouldThrowMockVerificationException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
				.Within(TimeSpan.FromMilliseconds(20)).Twice();
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(It.IsAny<string>(), It.IsAny<int>()) exactly twice, but it timed out after 00:00:00.0200000.");
	}

	private class MyChocolateDispenser : IChocolateDispenser
	{
		private readonly Dictionary<string, int> _inventory = new();

		public int this[string type]
		{
			get
			{
				if (_inventory.TryGetValue(type, out int index))
				{
					return index;
				}

				return 0;
			}
			set => _inventory[type] = this[type] + value;
		}

		public int TotalDispensed { get; set; }

		public event ChocolateDispensedDelegate? ChocolateDispensed;

		public bool Dispense(string type, int amount)
		{
			if (this[type] > amount)
			{
				this[type] -= amount;
				ChocolateDispensed?.Invoke(type, amount);
				return true;
			}

			return false;
		}
	}

	internal static void ExecuteDoSomethingOn(IChocolateDispenser mock, int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			mock.Dispense("Dark", i);
		}
	}
}
