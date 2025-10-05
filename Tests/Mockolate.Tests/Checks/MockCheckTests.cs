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

	public interface IMyService
	{
		void DoSomething(int value);
	}
}
