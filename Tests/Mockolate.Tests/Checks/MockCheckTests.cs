using System;
using System.Collections.Generic;
using System.Text;

namespace Mockolate.Tests.Checks;

public class MockCheckTests
{
	[Fact]
	public async Task AllInteractionsVerified_WithoutInteractions_ShouldReturnTrue()
	{
		var sut = Mock.For<IMyService>();

		await That(sut.Check.AllInteractionsVerified).IsTrue();
	}

	[Fact]
	public async Task AllInteractionsVerified_WithUnverifiedInteractions_ShouldReturnFalse()
	{
		var sut = Mock.For<IMyService>();

		sut.Object.DoSomething(1);
		sut.Object.DoSomething(2);

		await That(sut.Invoked.DoSomething(1).Once());
		await That(sut.Check.AllInteractionsVerified).IsFalse();
	}

	[Fact]
	public async Task AllInteractionsVerified_WithVerifiedInteractions_ShouldReturnTrue()
	{
		var sut = Mock.For<IMyService>();

		sut.Object.DoSomething(1);
		sut.Object.DoSomething(2);

		await That(sut.Invoked.DoSomething(With.Any<int>()).AtLeastOnce());
		await That(sut.Check.AllInteractionsVerified).IsTrue();
	}

	[Fact]
	public async Task Then_ShouldVerifyInOrder()
	{
		var sut = Mock.For<IMyService>();

		sut.Object.DoSomething(1);
		sut.Object.DoSomething(2);
		sut.Object.DoSomething(3);
		sut.Object.DoSomething(4);

		await That(sut.Invoked.DoSomething(3).Then(m => m.Invoked.DoSomething(4)));
		await That(sut.Invoked.DoSomething(2).Then(m => m.Invoked.DoSomething(1))).IsFalse();
		await That(sut.Invoked.DoSomething(1).Then(m => m.Invoked.DoSomething(2), m => m.Invoked.DoSomething(3)));
	}

	[Fact]
	public async Task Then_WhenNoMatch_ShouldReturnFalse()
	{
		var sut = Mock.For<IMyService>();

		sut.Object.DoSomething(1);
		sut.Object.DoSomething(2);
		sut.Object.DoSomething(3);
		sut.Object.DoSomething(4);

		await That(sut.Invoked.DoSomething(6).Then(m => m.Invoked.DoSomething(4))).IsFalse();
		await That(sut.Invoked.DoSomething(1).Then(m => m.Invoked.DoSomething(6), m => m.Invoked.DoSomething(3))).IsFalse();
		await That(sut.Invoked.DoSomething(1).Then(m => m.Invoked.DoSomething(2), m => m.Invoked.DoSomething(6))).IsFalse();
	}

	public interface IMyService
	{
		void DoSomething(int value);
	}
}
