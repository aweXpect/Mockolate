using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Verify;

public class MockVerifyTests
{
	[Fact]
	public async Task AllInteractionsVerified_WithoutInteractions_ShouldReturnTrue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		await That(sut.VerifyMock.ThatAllInteractionsAreVerified()).IsTrue();
	}

	[Fact]
	public async Task AllInteractionsVerified_WithUnverifiedInteractions_ShouldReturnFalse()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		sut.Dispense("Dark", 1);
		sut.Dispense("Dark", 2);

		await That(sut.VerifyMock.Invoked.Dispense(With("Dark"), With(1))).Once();
		await That(sut.VerifyMock.ThatAllInteractionsAreVerified()).IsFalse();
	}

	[Fact]
	public async Task AllInteractionsVerified_WithVerifiedInteractions_ShouldReturnTrue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		sut.Dispense("Dark", 1);
		sut.Dispense("Dark", 2);

		await That(sut.VerifyMock.Invoked.Dispense(WithAny<string>(), WithAny<int>())).AtLeastOnce();
		await That(sut.VerifyMock.ThatAllInteractionsAreVerified()).IsTrue();
	}
}
