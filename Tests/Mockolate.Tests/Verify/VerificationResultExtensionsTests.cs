using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public class VerificationResultExtensionsTests
{
	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(2, 3, false)]
	[InlineData(2, 2, true)]
	[InlineData(2, 1, true)]
	public async Task AtLeast_ShouldReturnExpectedResult(int count, int times, bool expectSuccess)
	{
		Mock<IChocolateDispenser> mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.Dispense(WithAny<string>(), WithAny<int>()).AtLeast(times);

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				"Expected that mock invoked method Dispense(WithAny<string>(), WithAny<int>()) at least 3 times, but it did twice.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(2, true)]
	[InlineData(3, true)]
	public async Task AtLeastOnce_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		Mock<IChocolateDispenser> mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.Dispense(WithAny<string>(), WithAny<int>()).AtLeastOnce();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				"Expected that mock invoked method Dispense(WithAny<string>(), WithAny<int>()) at least once, but it never did.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, false)]
	[InlineData(2, true)]
	[InlineData(3, true)]
	public async Task AtLeastTwice_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		Mock<IChocolateDispenser> mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.Dispense(WithAny<string>(), WithAny<int>()).AtLeastTwice();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(WithAny<string>(), WithAny<int>()) at least twice, but it {count switch { 0 => "never did", _ => "did once", }}.");
	}

	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(2, 1, false)]
	[InlineData(2, 2, true)]
	[InlineData(2, 3, true)]
	public async Task AtMost_ShouldReturnExpectedResult(int count, int times, bool expectSuccess)
	{
		Mock<IChocolateDispenser> mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.Dispense(WithAny<string>(), WithAny<int>()).AtMost(times);

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				"Expected that mock invoked method Dispense(WithAny<string>(), WithAny<int>()) at most once, but it did twice.");
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public async Task AtMostOnce_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		Mock<IChocolateDispenser> mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.Dispense(WithAny<string>(), WithAny<int>()).AtMostOnce();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(WithAny<string>(), WithAny<int>()) at most once, but it did {(count == 2 ? "twice" : $"{count} times")}.");
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, true)]
	[InlineData(2, true)]
	[InlineData(3, false)]
	public async Task AtMostTwice_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		Mock<IChocolateDispenser> mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.Dispense(WithAny<string>(), WithAny<int>()).AtMostTwice();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(WithAny<string>(), WithAny<int>()) at most twice, but it did {count} times.");
	}

	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(2, 3, false)]
	[InlineData(2, 2, true)]
	[InlineData(2, 1, false)]
	public async Task Exactly_ShouldReturnExpectedResult(int count, int times, bool expectSuccess)
	{
		Mock<IChocolateDispenser> mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.Dispense(WithAny<string>(), WithAny<int>()).Exactly(times);

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(WithAny<string>(), WithAny<int>()) exactly {(times == 1 ? "once" : $"{times} times")}, but it did twice.");
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, false)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public async Task Never_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		Mock<IChocolateDispenser> mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.Dispense(WithAny<string>(), WithAny<int>()).Never();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock never invoked method Dispense(WithAny<string>(), WithAny<int>()), but it did {count switch { 1 => "once", 2 => "twice", _ => $"{count} times", }}.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public async Task Once_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		Mock<IChocolateDispenser> mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.Dispense(WithAny<string>(), WithAny<int>()).Once();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(WithAny<string>(), WithAny<int>()) exactly once, but it {count switch { 0 => "never did", 2 => "did twice", _ => $"did {count} times", }}.");
	}

	[Fact]
	public async Task Then_ShouldVerifyInOrder()
	{
		Mock<IChocolateDispenser> mock = Mock.Create<IChocolateDispenser>();
		mock.Subject.Dispense("Dark", 1);
		mock.Subject.Dispense("Dark", 2);
		mock.Subject.Dispense("Dark", 3);
		mock.Subject.Dispense("Dark", 4);

		mock.Verify.Invoked.Dispense(WithAny<string>(), With(3)).Then(m => m.Invoked.Dispense(WithAny<string>(), With(4)));

		void Act()
			=> mock.Verify.Invoked.Dispense(WithAny<string>(), With(2)).Then(m => m.Invoked.Dispense(WithAny<string>(), With(1)));

		await That(Act).Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(WithAny<string>(), 2), then invoked method Dispense(WithAny<string>(), 1) in order, but it invoked method Dispense(WithAny<string>(), 1) too early.");
		mock.Verify.Invoked.Dispense(WithAny<string>(), With(1)).Then(m => m.Invoked.Dispense(WithAny<string>(), With(2)));
	}

	[Fact]
	public async Task Then_WhenNoMatch_ShouldFail()
	{
		Mock<IChocolateDispenser> mock = Mock.Create<IChocolateDispenser>();
		mock.Subject.Dispense("Dark", 1);
		mock.Subject.Dispense("Dark", 2);
		mock.Subject.Dispense("Dark", 3);
		mock.Subject.Dispense("Dark", 4);

		await That(void () => mock.Verify.Invoked.Dispense(WithAny<string>(), With(6))
				.Then(m => m.Invoked.Dispense(WithAny<string>(), With(4))))
			.Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(WithAny<string>(), 6), then invoked method Dispense(WithAny<string>(), 4) in order, but it invoked method Dispense(WithAny<string>(), 6) not at all.");

		await That(void () => mock.Verify.Invoked.Dispense(WithAny<string>(), With(1))
				.Then(m => m.Invoked.Dispense(WithAny<string>(), With(6)), m => m.Invoked.Dispense(WithAny<string>(), With(3))))
			.Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(WithAny<string>(), 1), then invoked method Dispense(WithAny<string>(), 6), then invoked method Dispense(WithAny<string>(), 3) in order, but it invoked method Dispense(WithAny<string>(), 6) not at all.");

		await That(void () => mock.Verify.Invoked.Dispense(WithAny<string>(), With(1))
				.Then(m => m.Invoked.Dispense(WithAny<string>(), With(2)), m => m.Invoked.Dispense(WithAny<string>(), With(6))))
			.Throws<MockVerificationException>()
			.WithMessage(
				"Expected that mock invoked method Dispense(WithAny<string>(), 1), then invoked method Dispense(WithAny<string>(), 2), then invoked method Dispense(WithAny<string>(), 6) in order, but it invoked method Dispense(WithAny<string>(), 6) not at all.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, false)]
	[InlineData(2, true)]
	[InlineData(3, false)]
	public async Task Twice_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		Mock<IChocolateDispenser> mock = Mock.Create<IChocolateDispenser>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.Dispense(WithAny<string>(), WithAny<int>()).Twice();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage(
				$"Expected that mock invoked method Dispense(WithAny<string>(), WithAny<int>()) exactly twice, but it {count switch { 0 => "never did", 1 => "did once", _ => $"did {count} times", }}.");
	}

	internal static void ExecuteDoSomethingOn(Mock<IChocolateDispenser> mock, int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			mock.Subject.Dispense("Dark", i);
		}
	}
}
