using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.Verify;

public class MockVerifyTests
{
	[Fact]
	public async Task ThatAllInteractionsAreVerified_WithoutInteractions_ShouldReturnTrue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		await That(sut.VerifyMock.ThatAllInteractionsAreVerified()).IsTrue();
	}

	[Fact]
	public async Task ThatAllInteractionsAreVerified_WithUnverifiedInteractions_ShouldReturnFalse()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		sut.Dispense("Dark", 1);
		sut.Dispense("Dark", 2);

		await That(sut.VerifyMock.Invoked.Dispense(With("Dark"), With(1))).Once();
		await That(sut.VerifyMock.ThatAllInteractionsAreVerified()).IsFalse();
	}

	[Fact]
	public async Task ThatAllInteractionsAreVerified_WithVerifiedInteractions_ShouldReturnTrue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		sut.Dispense("Dark", 1);
		sut.Dispense("Dark", 2);

		await That(sut.VerifyMock.Invoked.Dispense(Any<string>(), Any<int>())).AtLeastOnce();
		await That(sut.VerifyMock.ThatAllInteractionsAreVerified()).IsTrue();
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(5, true)]
	public async Task ThatAllSetupsWereUsed_ShouldCheckIndexersWithGetter(int interactionCount, bool expectedResult)
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Indexer(Any<string>()).InitializeWith(1);

		for (int i = 0; i < interactionCount; i++)
		{
			_ = sut["Dark"];
		}

		await That(sut.VerifyMock.ThatAllSetupsWereUsed()).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(5, true)]
	public async Task ThatAllSetupsWereUsed_ShouldCheckIndexersWithSetter(int interactionCount, bool expectedResult)
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Indexer(Any<string>()).InitializeWith(1);

		for (int i = 0; i < interactionCount; i++)
		{
			sut["Dark"] = i;
		}

		await That(sut.VerifyMock.ThatAllSetupsWereUsed()).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(5, true)]
	public async Task ThatAllSetupsWereUsed_ShouldCheckMethods(int interactionCount, bool expectedResult)
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Method.Dispense(Any<string>(), Any<int>());

		for (int i = 0; i < interactionCount; i++)
		{
			sut.Dispense("Dark", i);
		}

		await That(sut.VerifyMock.ThatAllSetupsWereUsed()).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(5, true)]
	public async Task ThatAllSetupsWereUsed_ShouldCheckPropertiesWithGetter(int interactionCount, bool expectedResult)
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Property.TotalDispensed.InitializeWith(1);

		for (int i = 0; i < interactionCount; i++)
		{
			_ = sut.TotalDispensed;
		}

		await That(sut.VerifyMock.ThatAllSetupsWereUsed()).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(5, true)]
	public async Task ThatAllSetupsWereUsed_ShouldCheckPropertiesWithSetter(int interactionCount, bool expectedResult)
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Property.TotalDispensed.InitializeWith(1);

		for (int i = 0; i < interactionCount; i++)
		{
			sut.TotalDispensed = i;
		}

		await That(sut.VerifyMock.ThatAllSetupsWereUsed()).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ThatAllSetupsWereUsed_WithMultipleUsedSetups_ShouldReturnTrue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Method.Dispense(Any<string>(), With(1));
		sut.SetupMock.Method.Dispense(Any<string>(), With(2));

		sut.Dispense("Dark", 1);
		sut.Dispense("Dark", 2);
		sut.Dispense("Dark", 3);

		await That(sut.VerifyMock.ThatAllSetupsWereUsed()).IsTrue();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(5)]
	public async Task ThatAllSetupsWereUsed_WithoutSetups_ShouldReturnTrue(int interactionCount)
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		for (int i = 0; i < interactionCount; i++)
		{
			sut.Dispense("Dark", i);
		}

		await That(sut.VerifyMock.ThatAllSetupsWereUsed()).IsTrue();
	}

	[Fact]
	public async Task ThatAllSetupsWereUsed_WithPartlyUsedSetups_ShouldReturnFalse()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Method.Dispense(Any<string>(), With(1));
		sut.SetupMock.Method.Dispense(Any<string>(), With(2));
		sut.SetupMock.Method.Dispense(Any<string>(), With(3));
		sut.SetupMock.Method.Dispense(Any<string>(), With(4));

		sut.Dispense("Dark", 1);
		sut.Dispense("Dark", 3);

		await That(sut.VerifyMock.ThatAllSetupsWereUsed()).IsFalse();
	}

	[Fact]
	public async Task ThatAllSetupsWereUsed_WithUnusedSetup_ShouldReturnFalse()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Method.Dispense(Any<string>(), Any<int>());

		await That(sut.VerifyMock.ThatAllSetupsWereUsed()).IsFalse();
	}

	[Fact]
	public async Task ThatAllSetupsWereUsed_WithUsedSetup_ShouldReturnTrue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Method.Dispense(Any<string>(), Any<int>());

		sut.Dispense("Dark", 1);

		await That(sut.VerifyMock.ThatAllSetupsWereUsed()).IsTrue();
	}
}
