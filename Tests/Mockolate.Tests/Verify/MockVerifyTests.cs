using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Verify;

public class MockVerifyTests
{
	[Fact]
	public async Task AllInteractionsVerified_WithoutInteractions_ShouldReturnTrue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		await That(sut.Verify.ThatAllInteractionsAreVerified()).IsTrue();
	}

	[Fact]
	public async Task AllInteractionsVerified_WithUnverifiedInteractions_ShouldReturnFalse()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		sut.Subject.Dispense("Dark", 1);
		sut.Subject.Dispense("Dark", 2);

		await That(sut.Verify.Invoked.Dispense(With("Dark"), With(1))).Once();
		await That(sut.Verify.ThatAllInteractionsAreVerified()).IsFalse();
	}

	[Fact]
	public async Task AllInteractionsVerified_WithVerifiedInteractions_ShouldReturnTrue()
	{
		Mock<IChocolateDispenser> sut = Mock.Create<IChocolateDispenser>();

		sut.Subject.Dispense("Dark", 1);
		sut.Subject.Dispense("Dark", 2);

		await That(sut.Verify.Invoked.Dispense(WithAny<string>(), WithAny<int>())).AtLeastOnce();
		await That(sut.Verify.ThatAllInteractionsAreVerified()).IsTrue();
	}
}
