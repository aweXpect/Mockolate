using System.Collections.Generic;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public class VerificationResultExtensionsTests
{
	[Theory]
	[InlineData(0, 0, 0, true)]
	[InlineData(1, 0, 2, true)]
	[InlineData(2, 0, 2, true)]
	[InlineData(3, 2, 5, true)]
	[InlineData(1, 2, 5, false)]
	[InlineData(6, 2, 5, false)]
	[InlineData(2, 2, 2, true)]
	public async Task Between_ShouldReturnExpectedResult(int count, int min, int max, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).Between(min, max);
		}

		string expectedDidTimes = count switch
		{
			0 => "never did",
			1 => "did once",
			2 => "did twice",
			_ => $"did {count} times"
		};

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(Any<string>(), Any<int>()) between {min} and {max} times, but it {expectedDidTimes}.");
	}

	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(2, 3, false)]
	[InlineData(2, 2, true)]
	[InlineData(2, 1, true)]
	public async Task AtLeast_ShouldReturnExpectedResult(int count, int times, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).AtLeast(times);
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				"Expected that mock invoked method Dispense(Any<string>(), Any<int>()) at least 3 times, but it did twice.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(2, true)]
	[InlineData(3, true)]
	public async Task AtLeastOnce_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).AtLeastOnce();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				"Expected that mock invoked method Dispense(Any<string>(), Any<int>()) at least once, but it never did.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, false)]
	[InlineData(2, true)]
	[InlineData(3, true)]
	public async Task AtLeastTwice_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).AtLeastTwice();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(Any<string>(), Any<int>()) at least twice, but it {count switch { 0 => "never did", _ => "did once", }}.");
	}

	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(2, 1, false)]
	[InlineData(2, 2, true)]
	[InlineData(2, 3, true)]
	public async Task AtMost_ShouldReturnExpectedResult(int count, int times, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).AtMost(times);
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				"Expected that mock invoked method Dispense(Any<string>(), Any<int>()) at most once, but it did twice.");
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public async Task AtMostOnce_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).AtMostOnce();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(Any<string>(), Any<int>()) at most once, but it did {(count == 2 ? "twice" : $"{count} times")}.");
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, true)]
	[InlineData(2, true)]
	[InlineData(3, false)]
	public async Task AtMostTwice_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).AtMostTwice();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(Any<string>(), Any<int>()) at most twice, but it did {count} times.");
	}

	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(2, 3, false)]
	[InlineData(2, 2, true)]
	[InlineData(2, 1, false)]
	public async Task Exactly_ShouldReturnExpectedResult(int count, int times, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).Exactly(times);
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(Any<string>(), Any<int>()) exactly {(times == 1 ? "once" : $"{times} times")}, but it did twice.");
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, false)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public async Task Never_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).Never();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock never invoked method Dispense(Any<string>(), Any<int>()), but it did {count switch { 1 => "once", 2 => "twice", _ => $"{count} times", }}.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public async Task Once_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).Once();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(Any<string>(), Any<int>()) exactly once, but it {count switch { 0 => "never did", 2 => "did twice", _ => $"did {count} times", }}.");
	}

	[Fact]
	public async Task Then_ShouldVerifyInOrder()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		mock.Dispense("Dark", 1);
		mock.Dispense("Dark", 2);
		mock.Dispense("Dark", 3);
		mock.Dispense("Dark", 4);

		mock.VerifyMock.Invoked.Dispense(Any<string>(), With(3))
			.Then(m => m.Invoked.Dispense(Any<string>(), With(4)));

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), With(2))
				.Then(m => m.Invoked.Dispense(Any<string>(), With(1)));
		}

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(Any<string>(), 2), then invoked method Dispense(Any<string>(), 1) in order, but it invoked method Dispense(Any<string>(), 1) too early.");
		mock.VerifyMock.Invoked.Dispense(Any<string>(), With(1))
			.Then(m => m.Invoked.Dispense(Any<string>(), With(2)));
	}

	[Fact]
	public async Task Then_WhenNoMatch_ShouldFail()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		mock.Dispense("Dark", 1);
		mock.Dispense("Dark", 2);
		mock.Dispense("Dark", 3);
		mock.Dispense("Dark", 4);

		await That(void () => mock.VerifyMock.Invoked.Dispense(Any<string>(), With(6))
				.Then(m => m.Invoked.Dispense(Any<string>(), With(4))))
			.Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(Any<string>(), 6), then invoked method Dispense(Any<string>(), 4) in order, but it invoked method Dispense(Any<string>(), 6) not at all.");

		await That(void () => mock.VerifyMock.Invoked.Dispense(Any<string>(), With(1))
				.Then(m => m.Invoked.Dispense(Any<string>(), With(6)),
					m => m.Invoked.Dispense(Any<string>(), With(3))))
			.Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(Any<string>(), 1), then invoked method Dispense(Any<string>(), 6), then invoked method Dispense(Any<string>(), 3) in order, but it invoked method Dispense(Any<string>(), 6) not at all.");

		await That(void () => mock.VerifyMock.Invoked.Dispense(Any<string>(), With(1))
				.Then(m => m.Invoked.Dispense(Any<string>(), With(2)),
					m => m.Invoked.Dispense(Any<string>(), With(6))))
			.Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(Any<string>(), 1), then invoked method Dispense(Any<string>(), 2), then invoked method Dispense(Any<string>(), 6) in order, but it invoked method Dispense(Any<string>(), 6) not at all.");
	}

	[Fact]
	public async Task Then_WhenNoMock_ShouldThrowMockException()
	{
		IChocolateDispenser mock = new MyChocolateDispenser();
		VerificationResult<IChocolateDispenser> result = new(mock, new MockInteractions(), [], "foo");

		void Act()
		{
			result.Then(m => m.Invoked.Dispense(Any<string>(), With(1)));
		}

		await That(Act).Throws<MockException>()
			.WithMessage("The subject is no mock subject.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, false)]
	[InlineData(2, true)]
	[InlineData(3, false)]
	public async Task Twice_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).Twice();
		}

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(Any<string>(), Any<int>()) exactly twice, but it {count switch { 0 => "never did", 1 => "did once", _ => $"did {count} times", }}.");
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, false)]
	[InlineData(2, true)]
	[InlineData(3, false)]
	[InlineData(4, true)]
	[InlineData(5, false)]
	public async Task Times_WithEvenPredicate_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).Times(n => n % 2 == 0);
		}

		string expectedDidTimes = count switch
		{
			0 => "never did",
			1 => "did once",
			2 => "did twice",
			_ => $"did {count} times"
		};

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(Any<string>(), Any<int>()) a number of times matching the predicate, but it {expectedDidTimes}.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, false)]
	[InlineData(2, true)]
	[InlineData(3, true)]
	[InlineData(4, false)]
	[InlineData(5, true)]
	[InlineData(6, false)]
	[InlineData(7, true)]
	public async Task Times_WithPrimePredicate_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).Times(IsPrime);
		}

		string expectedDidTimes = count switch
		{
			0 => "never did",
			1 => "did once",
			2 => "did twice",
			_ => $"did {count} times"
		};

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(Any<string>(), Any<int>()) a number of times matching the predicate, but it {expectedDidTimes}.");

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

	[Fact]
	public async Task Between_WithNegativeMin_ShouldThrowArgumentOutOfRangeException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).Between(-1, 5);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("min");
	}

	[Fact]
	public async Task Between_WithMaxLessThanMin_ShouldThrowArgumentOutOfRangeException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).Between(5, 2);
		}

		await That(Act).Throws<ArgumentOutOfRangeException>()
			.WithParamName("max");
	}

	[Fact]
	public async Task Times_WithNullPredicate_ShouldThrowArgumentNullException()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();

		void Act()
		{
			mock.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>()).Times(null!);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithParamName("predicate");
	}

	internal static void ExecuteDoSomethingOn(IChocolateDispenser mock, int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			mock.Dispense("Dark", i);
		}
	}
}
