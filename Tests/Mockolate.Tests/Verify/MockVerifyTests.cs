using System.Collections.Generic;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public class MockVerifyTests
{
	[Fact]
	public async Task GetUnverifiedInteractions_ShouldBeSorted()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		MockInteractions interactions = ((IMockSubject<IChocolateDispenser>)sut).Mock.Interactions;

		sut.Dispense("Dark", 1);
		sut.Dispense("Dark", 2);
		IReadOnlyCollection<IInteraction> result1 = interactions.GetUnverifiedInteractions();

		sut.Dispense("Dark", 3);
		sut.Dispense("Dark", 4);
		IReadOnlyCollection<IInteraction> result2 = interactions.GetUnverifiedInteractions();

		await That(result1).IsInAscendingOrder(x => x.Index);
		await That(result2).IsInAscendingOrder(x => x.Index);
	}

	[Fact]
	public async Task GetUnverifiedInteractions_WhenVerified_ShouldBeSorted()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		MockInteractions interactions = ((IMockSubject<IChocolateDispenser>)sut).Mock.Interactions;

		sut.Dispense("Dark", 1);
		sut.Dispense("Dark", 2);
		sut.Dispense("Dark", 3);
		sut.Dispense("Dark", 4);

		sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.Is(5)).Never();
		IReadOnlyCollection<IInteraction> result = interactions.GetUnverifiedInteractions();

		await That(result).IsInAscendingOrder(x => x.Index);
	}

	[Fact]
	public async Task ThatAllInteractionsAreVerified_MultipleCalls_ShouldRepresentLatestValue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		bool result1 = sut.VerifyMock.ThatAllInteractionsAreVerified();
		sut.Dispense("Dark", 1);
		bool result2 = sut.VerifyMock.ThatAllInteractionsAreVerified();

		await That(result1).IsTrue();
		await That(result2).IsFalse();
	}

	[Fact]
	public async Task ThatAllInteractionsAreVerified_MultipleVerifications_ShouldBeCombined()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		sut.Dispense("Dark", 1);
		sut.Dispense("Dark", 2);

		sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.Is(1)).Once();
		sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.Is(2)).Once();

		await That(sut.VerifyMock.ThatAllInteractionsAreVerified()).IsTrue();
	}

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

		await That(sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(1))).Once();
		await That(sut.VerifyMock.ThatAllInteractionsAreVerified()).IsFalse();
	}

	[Fact]
	public async Task ThatAllInteractionsAreVerified_WithVerifiedInteractions_ShouldReturnTrue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		sut.Dispense("Dark", 1);
		sut.Dispense("Dark", 2);

		await That(sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())).AtLeastOnce();
		await That(sut.VerifyMock.ThatAllInteractionsAreVerified()).IsTrue();
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(5, true)]
	public async Task ThatAllSetupsAreUsed_ShouldCheckIndexersWithGetter(int interactionCount, bool expectedResult)
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Indexer(It.IsAny<string>()).InitializeWith(1);

		for (int i = 0; i < interactionCount; i++)
		{
			_ = sut["Dark"];
		}

		await That(sut.VerifyMock.ThatAllSetupsAreUsed()).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(5, true)]
	public async Task ThatAllSetupsAreUsed_ShouldCheckIndexersWithSetter(int interactionCount, bool expectedResult)
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Indexer(It.IsAny<string>()).InitializeWith(1);

		for (int i = 0; i < interactionCount; i++)
		{
			sut["Dark"] = i;
		}

		await That(sut.VerifyMock.ThatAllSetupsAreUsed()).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(5, true)]
	public async Task ThatAllSetupsAreUsed_ShouldCheckMethods(int interactionCount, bool expectedResult)
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>());

		for (int i = 0; i < interactionCount; i++)
		{
			sut.Dispense("Dark", i);
		}

		await That(sut.VerifyMock.ThatAllSetupsAreUsed()).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(5, true)]
	public async Task ThatAllSetupsAreUsed_ShouldCheckPropertiesWithGetter(int interactionCount, bool expectedResult)
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Property.TotalDispensed.InitializeWith(1);

		for (int i = 0; i < interactionCount; i++)
		{
			_ = sut.TotalDispensed;
		}

		await That(sut.VerifyMock.ThatAllSetupsAreUsed()).IsEqualTo(expectedResult);
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(1, true)]
	[InlineData(5, true)]
	public async Task ThatAllSetupsAreUsed_ShouldCheckPropertiesWithSetter(int interactionCount, bool expectedResult)
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Property.TotalDispensed.InitializeWith(1);

		for (int i = 0; i < interactionCount; i++)
		{
			sut.TotalDispensed = i;
		}

		await That(sut.VerifyMock.ThatAllSetupsAreUsed()).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ThatAllSetupsAreUsed_WithMultipleUsedSetups_ShouldReturnTrue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.Is(1));
		sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.Is(2));

		sut.Dispense("Dark", 1);
		sut.Dispense("Dark", 2);
		sut.Dispense("Dark", 3);

		await That(sut.VerifyMock.ThatAllSetupsAreUsed()).IsTrue();
	}

	[Fact]
	public async Task ThatAllSetupsAreUsed_WithoutSetup_ShouldReturnTrue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		sut.TotalDispensed = 0;

		await That(sut.VerifyMock.ThatAllSetupsAreUsed()).IsTrue();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(5)]
	public async Task ThatAllSetupsAreUsed_WithoutSetups_ShouldReturnTrue(int interactionCount)
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		for (int i = 0; i < interactionCount; i++)
		{
			sut.Dispense("Dark", i);
		}

		await That(sut.VerifyMock.ThatAllSetupsAreUsed()).IsTrue();
	}

	[Fact]
	public async Task ThatAllSetupsAreUsed_WithPartlyUsedSetups_ShouldReturnFalse()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.Is(1));
		sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.Is(2));
		sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.Is(3));
		sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.Is(4));

		sut.Dispense("Dark", 1);
		sut.Dispense("Dark", 3);

		await That(sut.VerifyMock.ThatAllSetupsAreUsed()).IsFalse();
	}

	[Fact]
	public async Task ThatAllSetupsAreUsed_WithUnusedSetup_ShouldReturnFalse()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>());

		await That(sut.VerifyMock.ThatAllSetupsAreUsed()).IsFalse();
	}

	[Fact]
	public async Task ThatAllSetupsAreUsed_WithUsedSetup_ShouldReturnTrue()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();
		sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>());

		sut.Dispense("Dark", 1);

		await That(sut.VerifyMock.ThatAllSetupsAreUsed()).IsTrue();
	}

	[Fact]
	public async Task VerificationResult_ShouldUpdateWhenInteractionsChange()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		IVerificationResult result = sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.IsAny<int>());
		bool r0 = result.Verify(f => f.Length == 0);
		sut.Dispense("Dark", 1);
		bool r1 = result.Verify(f => f.Length == 1);
		sut.Dispense("White", 2);
		bool r2 = result.Verify(f => f.Length == 1);
		sut.Dispense("Dark", 2);
		bool r3 = result.Verify(f => f.Length == 2);

		await That(r0).IsTrue().Because("No interaction was performed yet");
		await That(r1).IsTrue().Because("One interaction was performed");
		await That(r2).IsTrue().Because("The second interaction did not match");
		await That(r3).IsTrue().Because("The third interaction did again match");
	}
}
