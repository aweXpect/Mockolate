using Mockolate.Checks;

namespace Mockolate.Tests.Checks;

public class MockCheckTests
{
	[Fact]
	public async Task AllInteractionsVerified_WithoutInteractions_ShouldReturnTrue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		await That(sut.Check.AllInteractionsVerified()).IsTrue();
	}

	[Fact]
	public async Task AllInteractionsVerified_WithUnverifiedInteractions_ShouldReturnFalse()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		sut.Subject.DoSomething(1);
		sut.Subject.DoSomething(2);

		await That(sut.Verify.Invoked.DoSomething(1)).Once();
		await That(sut.Check.AllInteractionsVerified()).IsFalse();
	}

	[Fact]
	public async Task AllInteractionsVerified_WithVerifiedInteractions_ShouldReturnTrue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		sut.Subject.DoSomething(1);
		sut.Subject.DoSomething(2);

		await That(sut.Verify.Invoked.DoSomething(With.Any<int>())).AtLeastOnce();
		await That(sut.Check.AllInteractionsVerified()).IsTrue();
	}

	[Fact]
	public async Task Then_ShouldVerifyInOrder()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		sut.Subject.DoSomething(1);
		sut.Subject.DoSomething(2);
		sut.Subject.DoSomething(3);
		sut.Subject.DoSomething(4);

		await That(sut.Verify.Invoked.DoSomething(3).Then(m => m.Verify.Invoked.DoSomething(4)));
		await That(sut.Verify.Invoked.DoSomething(2).Then(m => m.Verify.Invoked.DoSomething(1))).IsFalse();
		await That(sut.Verify.Invoked.DoSomething(1).Then(m => m.Verify.Invoked.DoSomething(2), m => m.Verify.Invoked.DoSomething(3)));
	}

	[Fact]
	public async Task Then_WhenNoMatch_ShouldReturnFalse()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		sut.Subject.DoSomething(1);
		sut.Subject.DoSomething(2);
		sut.Subject.DoSomething(3);
		sut.Subject.DoSomething(4);

		await That(sut.Verify.Invoked.DoSomething(6).Then(m => m.Verify.Invoked.DoSomething(4))).IsFalse();
		await That(sut.Verify.Invoked.DoSomething(1).Then(m => m.Verify.Invoked.DoSomething(6), m => m.Verify.Invoked.DoSomething(3)))
			.IsFalse();
		await That(sut.Verify.Invoked.DoSomething(1).Then(m => m.Verify.Invoked.DoSomething(2), m => m.Verify.Invoked.DoSomething(6)))
			.IsFalse();
	}

	public interface IMyService
	{
		void DoSomething(int value);
	}
}
