using Mockolate;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public class MockVerifyTests
{
	[Fact]
	public async Task AllInteractionsVerified_WithoutInteractions_ShouldReturnTrue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		await That(sut.Verify.ThatAllInteractionsAreVerified()).IsTrue();
	}

	[Fact]
	public async Task AllInteractionsVerified_WithUnverifiedInteractions_ShouldReturnFalse()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		sut.Subject.DoSomething(1);
		sut.Subject.DoSomething(2);

		sut.Verify.Invoked.DoSomething(1).Once();
		await That(sut.Verify.ThatAllInteractionsAreVerified()).IsFalse();
	}

	[Fact]
	public async Task AllInteractionsVerified_WithVerifiedInteractions_ShouldReturnTrue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		sut.Subject.DoSomething(1);
		sut.Subject.DoSomething(2);

		sut.Verify.Invoked.DoSomething(With.Any<int>()).AtLeastOnce();
		await That(sut.Verify.ThatAllInteractionsAreVerified()).IsTrue();
	}

	public interface IMyService
	{
		void DoSomething(int value);
	}
}
