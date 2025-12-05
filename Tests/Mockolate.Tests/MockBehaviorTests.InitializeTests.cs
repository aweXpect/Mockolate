using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	public sealed class InitializeTests
	{
		[Fact]
		public async Task Initialize_DirectSetupsTakePrecedence()
		{
			MockBehavior behavior = MockBehavior.Default.Initialize<IChocolateDispenser>((counter, setup)
				=> setup.Indexer(It.Is<string>(s => s.StartsWith("da"))).InitializeWith(5));

			IChocolateDispenser mock = Mock.Create<IChocolateDispenser>(behavior,
				setup => setup.Indexer(It.Is<string>(s => s.EndsWith("rk"))).InitializeWith(16));

			int bothMatchResult = mock["dark"];
			int directMatchResult = mock["--rk"];
			int behaviorMatchResult = mock["da--"];
			int noneMatchResult = mock["foo"];

			await That(bothMatchResult).IsEqualTo(16);
			await That(directMatchResult).IsEqualTo(16);
			await That(behaviorMatchResult).IsEqualTo(5);
			await That(noneMatchResult).IsEqualTo(0);
		}

		[Fact]
		public async Task Initialize_ShouldApplySetupToCreatedMock()
		{
			MockBehavior behavior =
				MockBehavior.Default.Initialize<IChocolateDispenser>(setup
					=> setup.Indexer(It.Is("Dark")).InitializeWith(15));

			IChocolateDispenser mock = Mock.Create<IChocolateDispenser>(behavior);

			int setupResult = mock["Dark"];
			int otherResult = mock["Light"];

			await That(setupResult).IsEqualTo(15);
			await That(otherResult).IsEqualTo(0);
		}

		[Fact]
		public async Task WithCounter_ShouldIncrementForEachCreatedMock()
		{
			MockBehavior behavior = MockBehavior.Default.Initialize<IChocolateDispenser>((counter, setup)
				=> setup.Indexer(It.Is("Dark")).InitializeWith(counter));

			IChocolateDispenser mockA = Mock.Create<IChocolateDispenser>(behavior);
			IChocolateDispenser mockB = Mock.Create<IChocolateDispenser>(behavior);

			int resultA1 = mockA["Dark"];
			int resultA2 = mockA["Light"];
			int resultB1 = mockB["Dark"];
			int resultB2 = mockB["Light"];

			await That(resultA1).IsEqualTo(1);
			await That(resultA2).IsEqualTo(0);
			await That(resultB1).IsEqualTo(2);
			await That(resultB2).IsEqualTo(0);
		}
	}
}
