using System.Linq;
using aweXpect.Core;
using Mockolate;
using Mockolate.Exceptions;
using Mockolate.Interactions;
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
		var mock = Mock.Create<IMyService>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.DoSomething(With.Any<int>()).AtLeast(times);

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage("Expected that mock invoked method DoSomething(With.Any<int>()) at least 3 times, but it did twice.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(2, true)]
	[InlineData(3, true)]
	public async Task AtLeastOnce_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		var mock = Mock.Create<IMyService>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.DoSomething(With.Any<int>()).AtLeastOnce();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage("Expected that mock invoked method DoSomething(With.Any<int>()) at least once, but it never did.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, false)]
	[InlineData(2, true)]
	[InlineData(3, true)]
	public async Task AtLeastTwice_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		var mock = Mock.Create<IMyService>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.DoSomething(With.Any<int>()).AtLeastTwice();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage($"Expected that mock invoked method DoSomething(With.Any<int>()) at least twice, but it {(count switch { 0 => "never did", _ => "did once" })}.");
	}

	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(2, 1, false)]
	[InlineData(2, 2, true)]
	[InlineData(2, 3, true)]
	public async Task AtMost_ShouldReturnExpectedResult(int count, int times, bool expectSuccess)
	{
		var mock = Mock.Create<IMyService>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.DoSomething(With.Any<int>()).AtMost(times);

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage("Expected that mock invoked method DoSomething(With.Any<int>()) at most once, but it did twice.");
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public async Task AtMostOnce_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		var mock = Mock.Create<IMyService>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.DoSomething(With.Any<int>()).AtMostOnce();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage($"Expected that mock invoked method DoSomething(With.Any<int>()) at most once, but it did {(count == 2 ? "twice" : $"{count} times")}.");
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, true)]
	[InlineData(2, true)]
	[InlineData(3, false)]
	public async Task AtMostTwice_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		var mock = Mock.Create<IMyService>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.DoSomething(With.Any<int>()).AtMostTwice();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage($"Expected that mock invoked method DoSomething(With.Any<int>()) at most twice, but it did {count} times.");
	}

	[Theory]
	[InlineData(0, 0, true)]
	[InlineData(2, 3, false)]
	[InlineData(2, 2, true)]
	[InlineData(2, 1, false)]
	public async Task Exactly_ShouldReturnExpectedResult(int count, int times, bool expectSuccess)
	{
		var mock = Mock.Create<IMyService>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.DoSomething(With.Any<int>()).Exactly(times);

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage($"Expected that mock invoked method DoSomething(With.Any<int>()) exactly {(times == 1 ? "once" : $"{times} times")}, but it did twice.");
	}

	[Theory]
	[InlineData(0, true)]
	[InlineData(1, false)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public async Task Never_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		var mock = Mock.Create<IMyService>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.DoSomething(With.Any<int>()).Never();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage($"Expected that mock never invoked method DoSomething(With.Any<int>()), but it did {(count switch { 1 => "once", 2 => "twice", _ => $"{count} times" })}.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public async Task Once_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		var mock = Mock.Create<IMyService>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.DoSomething(With.Any<int>()).Once();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage($"Expected that mock invoked method DoSomething(With.Any<int>()) exactly once, but it {(count switch { 0 => "never did", 2 => "did twice", _ => $"did {count} times" })}.");
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, false)]
	[InlineData(2, true)]
	[InlineData(3, false)]
	public async Task Twice_ShouldReturnExpectedResult(int count, bool expectSuccess)
	{
		var mock = Mock.Create<IMyService>();
		ExecuteDoSomethingOn(mock, count);

		void Act()
			=> mock.Verify.Invoked.DoSomething(With.Any<int>()).Twice();

		await That(Act).Throws<MockVerificationException>().OnlyIf(!expectSuccess)
			.WithMessage($"Expected that mock invoked method DoSomething(With.Any<int>()) exactly twice, but it {(count switch { 0 => "never did", 1 => "did once", _ => $"did {count} times" })}.");
	}

	[Fact]
	public async Task Then_ShouldVerifyInOrder()
	{
		var mock = Mock.Create<IMyService>();
		mock.Subject.DoSomething(1);
		mock.Subject.DoSomething(2);
		mock.Subject.DoSomething(3);
		mock.Subject.DoSomething(4);

		mock.Verify.Invoked.DoSomething(3).Then(m => m.Verify.Invoked.DoSomething(4));
		void Act()
			=> mock.Verify.Invoked.DoSomething(2).Then(m => m.Verify.Invoked.DoSomething(1));

		await That(Act).Throws<MockVerificationException>()
			.WithMessage($"Expected that mock invoked method DoSomething(2), then invoked method DoSomething(1) in order, but it invoked method DoSomething(1) too early.");
		mock.Verify.Invoked.DoSomething(1).Then(m => m.Verify.Invoked.DoSomething(2));
	}

	[Fact]
	public async Task Then_WhenNoMatch_ShouldFail()
	{
		var mock = Mock.Create<IMyService>();
		mock.Subject.DoSomething(1);
		mock.Subject.DoSomething(2);
		mock.Subject.DoSomething(3);
		mock.Subject.DoSomething(4);

		await That(void () => mock.Verify.Invoked.DoSomething(6).Then(m => m.Verify.Invoked.DoSomething(4)))
			.Throws<MockVerificationException>()
			.WithMessage($"Expected that mock invoked method DoSomething(6), then invoked method DoSomething(4) in order, but it invoked method DoSomething(6) not at all.");
		
		await That(void () => mock.Verify.Invoked.DoSomething(1).Then(m => m.Verify.Invoked.DoSomething(6), m => m.Verify.Invoked.DoSomething(3)))
			.Throws<MockVerificationException>()
			.WithMessage($"Expected that mock invoked method DoSomething(1), then invoked method DoSomething(6), then invoked method DoSomething(3) in order, but it invoked method DoSomething(6) not at all.");
		
		await That(void () => mock.Verify.Invoked.DoSomething(1).Then(m => m.Verify.Invoked.DoSomething(2), m => m.Verify.Invoked.DoSomething(6)))
			.Throws<MockVerificationException>()
			.WithMessage($"Expected that mock invoked method DoSomething(1), then invoked method DoSomething(2), then invoked method DoSomething(6) in order, but it invoked method DoSomething(6) not at all.");
	}

	internal static void ExecuteDoSomethingOn(Mock<IMyService> mock, int amount)
	{
		for (int i= 0; i < amount; i++)
		{
			mock.Subject.DoSomething(i);
		}
	}

	private class MyInteraction : IInteraction
	{
		public int Index => 0;
	}
}
